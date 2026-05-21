export type HistoryItem = {
  id: string;
  filePath: string;
  createdAt: string;
};

export type ShortcutAction = "area" | "full";
export type ShortcutOption = {
  accelerator: string;
  label: string;
};
export type AppSettings = {
  shortcuts: Record<ShortcutAction, string>;
};
export type ShortcutOptions = Record<ShortcutAction, ShortcutOption[]>;
export type ShortcutRegistration = Record<ShortcutAction, boolean>;

declare global {
  interface Window {
    multisnap: {
      captureScreen: () => Promise<string>;
      captureFull: () => Promise<void>;
      copyImage: (dataUrl: string) => Promise<void>;
      deleteHistoryItem: (id: string) => Promise<void>;
      getHistory: () => Promise<HistoryItem[]>;
      getSettings: () => Promise<AppSettings>;
      getShortcutRegistration: () => Promise<ShortcutRegistration>;
      getShortcutOptions: () => Promise<ShortcutOptions>;
      hideOverlay: () => Promise<void>;
      openImage: (filePath: string) => Promise<void>;
      saveImage: (dataUrl: string) => Promise<HistoryItem | null>;
      setShortcut: (action: ShortcutAction, accelerator: string) => Promise<AppSettings>;
      showOverlay: () => Promise<void>;
      submitCapture: (dataUrl: string) => Promise<void>;
      onEditorImage: (callback: (dataUrl: string) => void) => () => void;
      onHistoryChanged: (callback: () => void) => () => void;
      onSettingsChanged: (callback: (settings: AppSettings) => void) => () => void;
      onShortcutRegistrationChanged: (callback: (registration: ShortcutRegistration) => void) => () => void;
    };
  }
}

export {};
