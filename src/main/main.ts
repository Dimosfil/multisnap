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

type ShortcutAction = "area" | "full";
type ShortcutOption = {
  accelerator: string;
  label: string;
};
type AppSettings = {
  shortcuts: Record<ShortcutAction, string>;
};
type AppSettingsUpdate = {
  shortcuts?: Partial<Record<ShortcutAction, string>>;
};
type ShortcutRegistration = Record<ShortcutAction, boolean>;

const shortcutOptions: Record<ShortcutAction, ShortcutOption[]> = {
  area: [
    { accelerator: "Control+PrintScreen", label: "Ctrl+Print Screen" },
    { accelerator: "PrintScreen", label: "Print Screen" },
    { accelerator: "CommandOrControl+Shift+5", label: "Ctrl+Shift+5" }
  ],
  full: [
    { accelerator: "CommandOrControl+Shift+6", label: "Ctrl+Shift+6" }
  ]
};

const defaultSettings: AppSettings = {
  shortcuts: {
    area: "Control+PrintScreen",
    full: "CommandOrControl+Shift+6"
  }
};

const legacyDefaultSettings: AppSettings = {
  shortcuts: {
    area: "PrintScreen",
    full: "Control+PrintScreen"
  }
};

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const isDev = !app.isPackaged;

let mainWindow: BrowserWindow | null = null;
let overlayWindow: BrowserWindow | null = null;
let tray: Tray | null = null;
let isQuitting = false;
let registeredShortcutAccelerators: string[] = [];
let pendingOverlayDataUrl: string | null = null;
let shortcutRegistration: ShortcutRegistration = {
  area: false,
  full: false
};

const rendererUrl = isDev
  ? "http://127.0.0.1:5187"
  : `file://${path.join(__dirname, "../renderer/index.html")}`;

function getPreloadPath() {
  return path.join(__dirname, "preload.js");
}

function historyPath() {
  return path.join(app.getPath("userData"), "history.json");
}

function settingsPath() {
  return path.join(app.getPath("userData"), "settings.json");
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

function isShortcutAction(value: string): value is ShortcutAction {
  return value === "area" || value === "full";
}

function sanitizeShortcut(action: ShortcutAction, accelerator: unknown) {
  const value = typeof accelerator === "string" ? accelerator : defaultSettings.shortcuts[action];
  return shortcutOptions[action].some((option) => option.accelerator === value)
    ? value
    : defaultSettings.shortcuts[action];
}

function sanitizeSettings(value: unknown): AppSettings {
  const candidate = value as Partial<AppSettings> | null;
  return {
    shortcuts: {
      area: sanitizeShortcut("area", candidate?.shortcuts?.area),
      full: sanitizeShortcut("full", candidate?.shortcuts?.full)
    }
  };
}

function isLegacyDefaultSettings(value: unknown) {
  const candidate = value as Partial<AppSettings> | null;
  return (
    candidate?.shortcuts?.area === legacyDefaultSettings.shortcuts.area &&
    candidate.shortcuts.full === legacyDefaultSettings.shortcuts.full
  );
}

async function readSettings(): Promise<AppSettings> {
  try {
    const raw = await fs.readFile(settingsPath(), "utf8");
    const parsed = JSON.parse(raw);
    if (isLegacyDefaultSettings(parsed)) {
      await writeSettings(defaultSettings);
      return defaultSettings;
    }
    return sanitizeSettings(parsed);
  } catch {
    return defaultSettings;
  }
}

async function writeSettings(settings: AppSettings) {
  await fs.mkdir(path.dirname(settingsPath()), { recursive: true });
  await fs.writeFile(settingsPath(), JSON.stringify(settings, null, 2), "utf8");
}

function imageFromDataUrl(dataUrl: string) {
  return nativeImage.createFromDataURL(dataUrl);
}

function timestampName() {
  const stamp = new Date().toISOString().replace(/[:.]/g, "-");
  return `multisnap-${stamp}.png`;
}

function getCursorDisplay() {
  return screen.getDisplayNearestPoint(screen.getCursorScreenPoint());
}

async function captureDisplay(display = getCursorDisplay()) {
  const size = display.size;
  const sources = await desktopCapturer.getSources({
    types: ["screen"],
    thumbnailSize: {
      width: Math.round(size.width * display.scaleFactor),
      height: Math.round(size.height * display.scaleFactor)
    }
  });

  const source =
    sources.find((item) => item.display_id === String(display.id)) ??
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
    show: false,
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

  const display = getCursorDisplay();
  pendingOverlayDataUrl = await captureDisplay(display);
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
    pendingOverlayDataUrl = null;
  });

  await overlayWindow.loadURL(`${rendererUrl}#/overlay`);
}

function createTray() {
  const icon = nativeImage.createFromDataURL(
    `data:image/svg+xml,${encodeURIComponent(`
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32">
        <rect x="3" y="3" width="26" height="26" rx="6" fill="#1f232b"/>
        <rect x="8" y="8" width="16" height="16" rx="3" fill="#ef1f2d"/>
      </svg>
    `)}`
  );
  tray = new Tray(icon);
  tray.setToolTip("MultiSnap");
  tray.setContextMenu(
    Menu.buildFromTemplate([
      { label: "Выделить область", click: () => createOverlayWindow() },
      { label: "Снять весь экран", click: () => captureFullToEditor() },
      { type: "separator" },
      { label: "Показать MultiSnap", click: () => showMainWindow() },
      { label: "Выход", click: () => app.quit() }
    ])
  );
  tray.on("click", () => createOverlayWindow());
  tray.on("double-click", () => showMainWindow());
}

function showMainWindow() {
  mainWindow?.show();
  mainWindow?.focus();
}

async function captureFullToEditor() {
  const dataUrl = await captureDisplay();
  clipboard.writeImage(imageFromDataUrl(dataUrl));
  showMainWindow();
  mainWindow?.webContents.send("editor-image", dataUrl);
}

async function getOverlayCapture() {
  return pendingOverlayDataUrl ?? captureDisplay();
}

async function registerShortcuts() {
  registeredShortcutAccelerators.forEach((accelerator) => globalShortcut.unregister(accelerator));
  registeredShortcutAccelerators = [];
  shortcutRegistration = {
    area: false,
    full: false
  };
  const settings = await readSettings();
  const handlers: Record<ShortcutAction, () => void> = {
    area: () => {
      void createOverlayWindow();
    },
    full: () => {
      void captureFullToEditor();
    }
  };

  (Object.keys(settings.shortcuts) as ShortcutAction[]).forEach((action) => {
    const accelerator = settings.shortcuts[action];
    if (globalShortcut.register(accelerator, handlers[action])) {
      registeredShortcutAccelerators.push(accelerator);
      shortcutRegistration[action] = true;
    }
  });
  mainWindow?.webContents.send("shortcut-registration-changed", shortcutRegistration);
}

async function updateSettings(nextSettings: AppSettingsUpdate) {
  const current = await readSettings();
  const settings = sanitizeSettings({
    shortcuts: {
      ...current.shortcuts,
      ...nextSettings.shortcuts
    }
  });
  await writeSettings(settings);
  await registerShortcuts();
  mainWindow?.webContents.send("settings-changed", settings);
  return settings;
}

function getShortcutOptions() {
  return shortcutOptions;
}

function getShortcutRegistration() {
  return shortcutRegistration;
}

function actionFromShortcutChannel(value: unknown): ShortcutAction | null {
  return typeof value === "string" && isShortcutAction(value) ? value : null;
}

function setShortcut(actionValue: unknown, accelerator: unknown) {
  const action = actionFromShortcutChannel(actionValue);
  if (!action) {
    throw new Error("Unknown shortcut action.");
  }
  return updateSettings({
    shortcuts: {
      [action]: sanitizeShortcut(action, accelerator)
    }
  });
}

app.whenReady().then(async () => {
  await createMainWindow();
  createTray();
  await registerShortcuts();
});

app.on("will-quit", () => {
  isQuitting = true;
  globalShortcut.unregisterAll();
});

ipcMain.handle("capture-screen", getOverlayCapture);
ipcMain.handle("capture-full", captureFullToEditor);
ipcMain.handle("get-settings", readSettings);
ipcMain.handle("get-shortcut-options", getShortcutOptions);
ipcMain.handle("get-shortcut-registration", getShortcutRegistration);
ipcMain.handle("set-shortcut", (_event, action: unknown, accelerator: unknown) =>
  setShortcut(action, accelerator)
);
ipcMain.handle("show-overlay", createOverlayWindow);
ipcMain.handle("hide-overlay", () => {
  overlayWindow?.close();
});
ipcMain.handle("submit-capture", (_event, dataUrl: string) => {
  clipboard.writeImage(imageFromDataUrl(dataUrl));
  overlayWindow?.close();
  showMainWindow();
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
