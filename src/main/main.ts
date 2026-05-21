import {
  app,
  BrowserWindow,
  clipboard,
  desktopCapturer,
  dialog,
  globalShortcut,
  ipcMain,
  nativeImage,
  screen,
  shell,
  Tray,
  Menu
} from "electron";
import fs from "node:fs/promises";
import path from "node:path";
import { randomUUID } from "node:crypto";
import { fileURLToPath } from "node:url";

type HistoryItem = {
  id: string;
  filePath: string;
  createdAt: string;
};

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const isDev = !app.isPackaged;

let mainWindow: BrowserWindow | null = null;
let overlayWindow: BrowserWindow | null = null;
let tray: Tray | null = null;
let isQuitting = false;

const rendererUrl = isDev
  ? "http://127.0.0.1:5187"
  : `file://${path.join(__dirname, "../renderer/index.html")}`;

function getPreloadPath() {
  return path.join(__dirname, "preload.js");
}

function historyPath() {
  return path.join(app.getPath("userData"), "history.json");
}

async function ensureScreenshotDir() {
  const dir = path.join(app.getPath("pictures"), "MultiSnap");
  await fs.mkdir(dir, { recursive: true });
  return dir;
}

async function readHistory(): Promise<HistoryItem[]> {
  try {
    const raw = await fs.readFile(historyPath(), "utf8");
    return JSON.parse(raw) as HistoryItem[];
  } catch {
    return [];
  }
}

async function writeHistory(items: HistoryItem[]) {
  await fs.mkdir(path.dirname(historyPath()), { recursive: true });
  await fs.writeFile(historyPath(), JSON.stringify(items.slice(0, 50), null, 2), "utf8");
}

function imageFromDataUrl(dataUrl: string) {
  return nativeImage.createFromDataURL(dataUrl);
}

function timestampName() {
  const stamp = new Date().toISOString().replace(/[:.]/g, "-");
  return `multisnap-${stamp}.png`;
}

async function capturePrimaryScreen() {
  const primary = screen.getPrimaryDisplay();
  const size = primary.size;
  const sources = await desktopCapturer.getSources({
    types: ["screen"],
    thumbnailSize: {
      width: Math.round(size.width * primary.scaleFactor),
      height: Math.round(size.height * primary.scaleFactor)
    }
  });

  const source =
    sources.find((item) => item.display_id === String(primary.id)) ??
    sources.find((item) => item.name.toLowerCase().includes("screen")) ??
    sources[0];

  if (!source) {
    throw new Error("No screen source is available.");
  }

  return source.thumbnail.toDataURL();
}

async function saveDataUrl(dataUrl: string, explicitPath?: string) {
  const image = imageFromDataUrl(dataUrl);
  const outputPath = explicitPath ?? path.join(await ensureScreenshotDir(), timestampName());
  await fs.writeFile(outputPath, image.toPNG());

  const history = await readHistory();
  const item: HistoryItem = {
    id: randomUUID(),
    filePath: outputPath,
    createdAt: new Date().toISOString()
  };

  await writeHistory([item, ...history]);
  mainWindow?.webContents.send("history-changed");
  return item;
}

async function createMainWindow() {
  mainWindow = new BrowserWindow({
    width: 1120,
    height: 760,
    minWidth: 860,
    minHeight: 560,
    backgroundColor: "#111318",
    title: "MultiSnap",
    webPreferences: {
      preload: getPreloadPath(),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  mainWindow.on("close", (event) => {
    if (isQuitting) return;
    event.preventDefault();
    mainWindow?.hide();
  });

  await mainWindow.loadURL(rendererUrl);
}

async function createOverlayWindow() {
  if (overlayWindow) {
    overlayWindow.focus();
    return;
  }

  const display = screen.getPrimaryDisplay();
  overlayWindow = new BrowserWindow({
    x: display.bounds.x,
    y: display.bounds.y,
    width: display.bounds.width,
    height: display.bounds.height,
    frame: false,
    fullscreen: true,
    transparent: true,
    alwaysOnTop: true,
    skipTaskbar: true,
    movable: false,
    resizable: false,
    backgroundColor: "#00000000",
    webPreferences: {
      preload: getPreloadPath(),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  overlayWindow.on("closed", () => {
    overlayWindow = null;
  });

  await overlayWindow.loadURL(`${rendererUrl}#/overlay`);
}

function createTray() {
  const icon = nativeImage.createEmpty();
  tray = new Tray(icon);
  tray.setToolTip("MultiSnap");
  tray.setContextMenu(
    Menu.buildFromTemplate([
      { label: "Capture area", click: () => createOverlayWindow() },
      { label: "Capture full screen", click: () => captureFullToEditor() },
      { type: "separator" },
      { label: "Show MultiSnap", click: () => mainWindow?.show() },
      { label: "Quit", click: () => app.quit() }
    ])
  );
}

async function captureFullToEditor() {
  const dataUrl = await capturePrimaryScreen();
  mainWindow?.show();
  mainWindow?.webContents.send("editor-image", dataUrl);
}

function registerShortcuts() {
  globalShortcut.register("CommandOrControl+Shift+5", () => {
    void createOverlayWindow();
  });
  globalShortcut.register("CommandOrControl+Shift+6", () => {
    void captureFullToEditor();
  });
}

app.whenReady().then(async () => {
  await createMainWindow();
  createTray();
  registerShortcuts();
});

app.on("will-quit", () => {
  isQuitting = true;
  globalShortcut.unregisterAll();
});

ipcMain.handle("capture-screen", capturePrimaryScreen);
ipcMain.handle("capture-full", captureFullToEditor);
ipcMain.handle("show-overlay", createOverlayWindow);
ipcMain.handle("hide-overlay", () => {
  overlayWindow?.close();
});
ipcMain.handle("submit-capture", (_event, dataUrl: string) => {
  overlayWindow?.close();
  mainWindow?.show();
  mainWindow?.webContents.send("editor-image", dataUrl);
});
ipcMain.handle("copy-image", (_event, dataUrl: string) => {
  clipboard.writeImage(imageFromDataUrl(dataUrl));
});
ipcMain.handle("save-image", async (_event, dataUrl: string) => {
  const options = {
    defaultPath: path.join(await ensureScreenshotDir(), timestampName()),
    filters: [{ name: "PNG image", extensions: ["png"] }]
  };
  const result = mainWindow
    ? await dialog.showSaveDialog(mainWindow, options)
    : await dialog.showSaveDialog(options);

  if (result.canceled || !result.filePath) {
    return null;
  }

  return saveDataUrl(dataUrl, result.filePath);
});
ipcMain.handle("get-history", readHistory);
ipcMain.handle("delete-history-item", async (_event, id: string) => {
  const history = await readHistory();
  const item = history.find((entry) => entry.id === id);
  if (item) {
    await fs.unlink(item.filePath).catch(() => undefined);
  }
  await writeHistory(history.filter((entry) => entry.id !== id));
  mainWindow?.webContents.send("history-changed");
});
ipcMain.handle("open-image", (_event, filePathValue: string) => {
  shell.openPath(filePathValue);
});
