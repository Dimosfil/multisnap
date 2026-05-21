export type HistoryItem = {
  id: string;
  filePath: string;
  createdAt: string;
};

declare global {
  interface Window {
    multisnap: {
      captureScreen: () => Promise<string>;
      captureFull: () => Promise<void>;
      copyImage: (dataUrl: string) => Promise<void>;
      deleteHistoryItem: (id: string) => Promise<void>;
      getHistory: () => Promise<HistoryItem[]>;
      hideOverlay: () => Promise<void>;
      openImage: (filePath: string) => Promise<void>;
      saveImage: (dataUrl: string) => Promise<HistoryItem | null>;
      showOverlay: () => Promise<void>;
      submitCapture: (dataUrl: string) => Promise<void>;
      onEditorImage: (callback: (dataUrl: string) => void) => () => void;
      onHistoryChanged: (callback: () => void) => () => void;
    };
  }
}

export {};

