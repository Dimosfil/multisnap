import { contextBridge, ipcRenderer } from "electron";

contextBridge.exposeInMainWorld("multisnap", {
  captureScreen: () => ipcRenderer.invoke("capture-screen"),
  captureFull: () => ipcRenderer.invoke("capture-full"),
  copyImage: (dataUrl: string) => ipcRenderer.invoke("copy-image", dataUrl),
  deleteHistoryItem: (id: string) => ipcRenderer.invoke("delete-history-item", id),
  getHistory: () => ipcRenderer.invoke("get-history"),
  getSettings: () => ipcRenderer.invoke("get-settings"),
  getShortcutRegistration: () => ipcRenderer.invoke("get-shortcut-registration"),
  getShortcutOptions: () => ipcRenderer.invoke("get-shortcut-options"),
  hideOverlay: () => ipcRenderer.invoke("hide-overlay"),
  openImage: (filePath: string) => ipcRenderer.invoke("open-image", filePath),
  saveImage: (dataUrl: string) => ipcRenderer.invoke("save-image", dataUrl),
  setShortcut: (action: string, accelerator: string) =>
    ipcRenderer.invoke("set-shortcut", action, accelerator),
  showOverlay: () => ipcRenderer.invoke("show-overlay"),
  submitCapture: (dataUrl: string) => ipcRenderer.invoke("submit-capture", dataUrl),
  onEditorImage: (callback: (dataUrl: string) => void) => {
    const listener = (_event: Electron.IpcRendererEvent, dataUrl: string) => callback(dataUrl);
    ipcRenderer.on("editor-image", listener);
    return () => ipcRenderer.removeListener("editor-image", listener);
  },
  onHistoryChanged: (callback: () => void) => {
    const listener = () => callback();
    ipcRenderer.on("history-changed", listener);
    return () => ipcRenderer.removeListener("history-changed", listener);
  },
  onSettingsChanged: (callback: (settings: unknown) => void) => {
    const listener = (_event: Electron.IpcRendererEvent, settings: unknown) => callback(settings);
    ipcRenderer.on("settings-changed", listener);
    return () => ipcRenderer.removeListener("settings-changed", listener);
  },
  onShortcutRegistrationChanged: (callback: (registration: unknown) => void) => {
    const listener = (_event: Electron.IpcRendererEvent, registration: unknown) =>
      callback(registration);
    ipcRenderer.on("shortcut-registration-changed", listener);
    return () => ipcRenderer.removeListener("shortcut-registration-changed", listener);
  }
});
