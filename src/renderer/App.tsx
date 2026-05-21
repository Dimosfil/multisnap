import { useEffect, useMemo, useRef, useState } from "react";
import {
  ArrowUpRight,
  Clipboard,
  ClipboardCheck,
  Crop,
  Download,
  Eraser,
  FolderOpen,
  Image as ImageIcon,
  Keyboard,
  Minus,
  MousePointer2,
  Pencil,
  RectangleHorizontal,
  Save,
  Trash2,
  Type,
  Undo2,
  X
} from "lucide-react";
import type {
  AppSettings,
  HistoryItem,
  ShortcutAction,
  ShortcutOptions,
  ShortcutRegistration
} from "./electron";

type Point = { x: number; y: number };
type Tool = "pen" | "rect" | "arrow" | "text" | "blur";
type Stroke = {
  tool: Exclude<Tool, "select">;
  points: Point[];
  color: string;
  width: number;
  text?: string;
};

const colors = ["#ff4d4d", "#38d996", "#4da3ff", "#ffd166", "#ffffff", "#111318"];

export function App() {
  if (!window.multisnap) {
    return <BrowserFallback />;
  }

  const isOverlay = window.location.hash === "#/overlay";
  return isOverlay ? <OverlayCapture /> : <MainApp />;
}

function BrowserFallback() {
  return (
    <main className="browserFallback">
      <div>
        <h1>MultiSnap Electron</h1>
        <p>Dev renderer is ready. Capture, tray, and editor controls are available in Electron.</p>
      </div>
    </main>
  );
}

function MainApp() {
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [image, setImage] = useState<string | null>(null);
  const [settings, setSettings] = useState<AppSettings | null>(null);
  const [shortcutOptions, setShortcutOptions] = useState<ShortcutOptions | null>(null);
  const [shortcutRegistration, setShortcutRegistration] = useState<ShortcutRegistration | null>(null);

  const refreshHistory = async () => {
    setHistory(await window.multisnap.getHistory());
  };

  useEffect(() => {
    void refreshHistory();
    void window.multisnap.getSettings().then(setSettings);
    void window.multisnap.getShortcutOptions().then(setShortcutOptions);
    void window.multisnap.getShortcutRegistration().then(setShortcutRegistration);
    const offImage = window.multisnap.onEditorImage((dataUrl) => setImage(dataUrl));
    const offHistory = window.multisnap.onHistoryChanged(() => void refreshHistory());
    const offSettings = window.multisnap.onSettingsChanged(setSettings);
    const offShortcutRegistration =
      window.multisnap.onShortcutRegistrationChanged(setShortcutRegistration);
    return () => {
      offImage();
      offHistory();
      offSettings();
      offShortcutRegistration();
    };
  }, []);

  const updateShortcut = async (action: ShortcutAction, accelerator: string) => {
    setSettings(await window.multisnap.setShortcut(action, accelerator));
  };

  if (image) {
    return <Editor image={image} onClose={() => setImage(null)} onSaved={refreshHistory} />;
  }

  return (
    <main className="appShell">
      <aside className="sidePanel">
        <div className="brandBlock">
          <div className="brandMark" aria-hidden="true">
            <span className="markHook markHookTop" />
            <span className="markHook markHookBottom" />
            <span className="markTile markTileRight" />
            <span className="markTile markTileLeft" />
          </div>
          <div>
            <h1>MultiSnap</h1>
            <p>Capture, mark up, ship.</p>
          </div>
        </div>

        <div className="actionGrid">
          <button className="primaryButton" onClick={() => window.multisnap.showOverlay()}>
            <Crop size={18} />
            Capture area
          </button>
          <button className="secondaryButton" onClick={() => window.multisnap.captureFull()}>
            <Download size={18} />
            Full screen
          </button>
        </div>

        {settings && shortcutOptions && (
          <SettingsPane
            options={shortcutOptions}
            registration={shortcutRegistration}
            settings={settings}
            onShortcutChange={updateShortcut}
          />
        )}

        <section className="historyPane">
          <div className="sectionTitle">
            <span>History</span>
            <button title="Refresh history" onClick={refreshHistory}>
              <ImageIcon size={16} />
            </button>
          </div>
          {history.length === 0 ? (
            <div className="emptyState">No screenshots yet.</div>
          ) : (
            <div className="historyList">
              {history.map((item) => (
                <HistoryRow
                  item={item}
                  key={item.id}
                  onDelete={async () => {
                    await window.multisnap.deleteHistoryItem(item.id);
                    await refreshHistory();
                  }}
                />
              ))}
            </div>
          )}
        </section>
      </aside>

      <section className="workspace">
        {image ? (
          <Editor image={image} onClose={() => setImage(null)} onSaved={refreshHistory} />
        ) : (
          <div className="startSurface">
            <MousePointer2 size={42} />
            <h2>Ready to capture</h2>
            <p>Use the buttons on the left or press Print Screen for area capture.</p>
          </div>
        )}
      </section>
    </main>
  );
}

function SettingsPane({
  options,
  registration,
  settings,
  onShortcutChange
}: {
  options: ShortcutOptions;
  registration: ShortcutRegistration | null;
  settings: AppSettings;
  onShortcutChange: (action: ShortcutAction, accelerator: string) => Promise<void>;
}) {
  return (
    <section className="settingsPane">
      <div className="sectionTitle">
        <span>Settings</span>
        <Keyboard size={16} />
      </div>
      <ShortcutSelect
        action="area"
        label="Area capture"
        options={options.area}
        isRegistered={registration?.area ?? false}
        value={settings.shortcuts.area}
        onChange={onShortcutChange}
      />
      <ShortcutSelect
        action="full"
        label="Full screen"
        options={options.full}
        isRegistered={registration?.full ?? false}
        value={settings.shortcuts.full}
        onChange={onShortcutChange}
      />
    </section>
  );
}

function ShortcutSelect({
  action,
  label,
  options,
  isRegistered,
  value,
  onChange
}: {
  action: ShortcutAction;
  label: string;
  options: ShortcutOptions[ShortcutAction];
  isRegistered: boolean;
  value: string;
  onChange: (action: ShortcutAction, accelerator: string) => Promise<void>;
}) {
  return (
    <label className="shortcutRow">
      <span>{label}</span>
      <select value={value} onChange={(event) => void onChange(action, event.target.value)}>
        {options.map((option) => (
          <option key={option.accelerator} value={option.accelerator}>
            {option.label}
          </option>
        ))}
      </select>
      <em className={isRegistered ? "shortcutStatus active" : "shortcutStatus blocked"}>
        {isRegistered ? "Active" : "Unavailable"}
      </em>
    </label>
  );
}

function HistoryRow({
  item,
  onDelete
}: {
  item: HistoryItem;
  onDelete: () => Promise<void>;
}) {
  const name = item.filePath.split(/[\\/]/).pop();
  const date = new Date(item.createdAt).toLocaleString();
  return (
    <article className="historyItem">
      <button className="historyText" onClick={() => window.multisnap.openImage(item.filePath)}>
        <strong>{name}</strong>
        <span>{date}</span>
      </button>
      <button title="Open file" onClick={() => window.multisnap.openImage(item.filePath)}>
        <FolderOpen size={16} />
      </button>
      <button title="Delete file" onClick={onDelete}>
        <Trash2 size={16} />
      </button>
    </article>
  );
}

function OverlayCapture() {
  const [screenshot, setScreenshot] = useState<string | null>(null);
  const [start, setStart] = useState<Point | null>(null);
  const [current, setCurrent] = useState<Point | null>(null);
  const imageRef = useRef<HTMLImageElement | null>(null);

  useEffect(() => {
    window.multisnap.captureScreen().then(setScreenshot).catch(() => window.multisnap.hideOverlay());
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") void window.multisnap.hideOverlay();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const rect = useMemo(() => {
    if (!start || !current) return null;
    const left = Math.min(start.x, current.x);
    const top = Math.min(start.y, current.y);
    return {
      left,
      top,
      width: Math.abs(start.x - current.x),
      height: Math.abs(start.y - current.y)
    };
  }, [start, current]);

  const cropSelection = async () => {
    if (!rect || rect.width < 8 || rect.height < 8 || !imageRef.current) return;

    const image = imageRef.current;
    const scaleX = image.naturalWidth / window.innerWidth;
    const scaleY = image.naturalHeight / window.innerHeight;
    const canvas = document.createElement("canvas");
    canvas.width = Math.round(rect.width * scaleX);
    canvas.height = Math.round(rect.height * scaleY);
    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    ctx.drawImage(
      image,
      Math.round(rect.left * scaleX),
      Math.round(rect.top * scaleY),
      canvas.width,
      canvas.height,
      0,
      0,
      canvas.width,
      canvas.height
    );
    await window.multisnap.submitCapture(canvas.toDataURL("image/png"));
  };

  const resetSelection = () => {
    setStart(null);
    setCurrent(null);
  };

  return (
    <div
      className="overlayRoot"
      onMouseDown={(event) => {
        if (event.button !== 0) {
          event.preventDefault();
          resetSelection();
          return;
        }
        setStart({ x: event.clientX, y: event.clientY });
        setCurrent({ x: event.clientX, y: event.clientY });
      }}
      onMouseMove={(event) => {
        if (!start) return;
        if (!event.shiftKey) {
          setCurrent({ x: event.clientX, y: event.clientY });
          return;
        }
        const deltaX = event.clientX - start.x;
        const deltaY = event.clientY - start.y;
        const side = Math.max(Math.abs(deltaX), Math.abs(deltaY));
        setCurrent({
          x: start.x + Math.sign(deltaX || 1) * side,
          y: start.y + Math.sign(deltaY || 1) * side
        });
      }}
      onMouseUp={(event) => {
        if (event.button !== 0) return;
        void cropSelection();
      }}
      onContextMenu={(event) => {
        event.preventDefault();
        resetSelection();
      }}
    >
      {screenshot && (
        <>
          <img ref={imageRef} src={screenshot} className="overlayShot" draggable={false} />
          {!rect && <div className="overlayShade" />}
        </>
      )}
      {rect && (
        <div
          className="selectionRect"
          style={{
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
          }}
        >
          <span>
            {Math.round(rect.width)} x {Math.round(rect.height)}
          </span>
        </div>
      )}
      <div className="overlayInfo">
        <strong>Щелкните и потяните, чтобы снять область экрана</strong>
        <span>Shift - квадратная область</span>
        <span>Esc - отмена</span>
      </div>
    </div>
  );
}

function Editor({
  image,
  onClose,
  onSaved
}: {
  image: string;
  onClose: () => void;
  onSaved: () => Promise<void>;
}) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const baseImageRef = useRef<HTMLImageElement | null>(null);
  const copyResetTimerRef = useRef<number | null>(null);
  const [tool, setTool] = useState<Tool>("pen");
  const [color, setColor] = useState(colors[0]);
  const [width, setWidth] = useState(4);
  const [strokes, setStrokes] = useState<Stroke[]>([]);
  const [draft, setDraft] = useState<Stroke | null>(null);
  const [copyState, setCopyState] = useState<"idle" | "copied">("idle");

  const draw = () => {
    const canvas = canvasRef.current;
    const base = baseImageRef.current;
    if (!canvas || !base) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;
    canvas.width = base.naturalWidth;
    canvas.height = base.naturalHeight;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(base, 0, 0);
    [...strokes, draft].filter(Boolean).forEach((stroke) => renderStroke(ctx, stroke as Stroke));
  };

  useEffect(() => {
    const img = new Image();
    img.onload = () => {
      baseImageRef.current = img;
      draw();
    };
    img.src = image;
  }, [image]);

  useEffect(draw, [strokes, draft]);

  useEffect(() => {
    return () => {
      if (copyResetTimerRef.current) {
        window.clearTimeout(copyResetTimerRef.current);
      }
    };
  }, []);

  const canvasPoint = (event: React.PointerEvent<HTMLCanvasElement>): Point => {
    const canvas = canvasRef.current!;
    const box = canvas.getBoundingClientRect();
    return {
      x: ((event.clientX - box.left) / box.width) * canvas.width,
      y: ((event.clientY - box.top) / box.height) * canvas.height
    };
  };

  const exportImage = () => {
    const base = baseImageRef.current;
    if (!base) return canvasRef.current?.toDataURL("image/png") ?? image;

    const canvas = document.createElement("canvas");
    canvas.width = base.naturalWidth;
    canvas.height = base.naturalHeight;
    const ctx = canvas.getContext("2d");
    if (!ctx) return image;

    ctx.drawImage(base, 0, 0);
    [...strokes, draft].filter(Boolean).forEach((stroke) => renderStroke(ctx, stroke as Stroke));
    return canvas.toDataURL("image/png");
  };

  const copyEditedImage = async () => {
    await window.multisnap.copyImage(exportImage());
    setCopyState("copied");
    if (copyResetTimerRef.current) {
      window.clearTimeout(copyResetTimerRef.current);
    }
    copyResetTimerRef.current = window.setTimeout(() => setCopyState("idle"), 1400);
  };

  const saveEditedImage = async () => {
    const saved = await window.multisnap.saveImage(exportImage());
    if (saved) await onSaved();
  };

  return (
    <div className="editorShell">
      <header className="editorTitlebar">
        <button className="topIconButton" title="Undo" onClick={() => setStrokes((items) => items.slice(0, -1))}>
          <Undo2 size={17} />
        </button>
        <div className="editorTitle">Monosnap Codex {new Date().toLocaleString()}</div>
        <div className="editorTopActions">
          <button className="titleActionButton copyTitleButton" title="Copy edited screenshot to clipboard" onClick={() => void copyEditedImage()}>
            {copyState === "copied" ? <ClipboardCheck size={17} /> : <Clipboard size={17} />}
            {copyState === "copied" ? "Copied" : "Copy"}
          </button>
          <button className="titleActionButton" onClick={() => void saveEditedImage()}>
            <Save size={17} />
            Save PNG
          </button>
          <button className="titleActionButton" title="Close editor" onClick={onClose}>
            <X size={17} />
            Close
          </button>
        </div>
      </header>

      <div className="canvasStage">
        <canvas
          ref={canvasRef}
          onPointerDown={(event) => {
            const point = canvasPoint(event);
            if (tool === "text") {
              const text = window.prompt("Text")?.trim();
              if (text) setStrokes((items) => [...items, { tool, points: [point], color, width, text }]);
              return;
            }
            event.currentTarget.setPointerCapture(event.pointerId);
            setDraft({ tool, points: [point], color, width });
          }}
          onPointerMove={(event) => {
            if (!draft) return;
            const point = canvasPoint(event);
            setDraft((item) => (item ? { ...item, points: [...item.points, point] } : item));
          }}
          onPointerUp={() => {
            if (draft) setStrokes((items) => [...items, draft]);
            setDraft(null);
          }}
        />
      </div>

      <footer className="toolbar bottomToolbar">
        <div className="toolbarTools">
          <ToolButton active={tool === "pen"} label="Pen" onClick={() => setTool("pen")}>
            <Pencil size={17} />
          </ToolButton>
          <ToolButton active={tool === "rect"} label="Rectangle" onClick={() => setTool("rect")}>
            <RectangleHorizontal size={17} />
          </ToolButton>
          <ToolButton active={tool === "arrow"} label="Arrow" onClick={() => setTool("arrow")}>
            <ArrowUpRight size={17} />
          </ToolButton>
          <ToolButton active={tool === "text"} label="Text" onClick={() => setTool("text")}>
            <Type size={17} />
          </ToolButton>
          <ToolButton active={tool === "blur"} label="Blur" onClick={() => setTool("blur")}>
            <Eraser size={17} />
          </ToolButton>
          <div className="divider" />
          <div className="swatches">
            {colors.map((item) => (
              <button
                aria-label={`Use ${item}`}
                className={item === color ? "swatch active" : "swatch"}
                key={item}
                onClick={() => setColor(item)}
                style={{ backgroundColor: item }}
              />
            ))}
          </div>
          <label className="widthControl">
            <Minus size={14} />
            <input
              max={18}
              min={2}
              type="range"
              value={width}
              onChange={(event) => setWidth(Number(event.target.value))}
            />
          </label>
        </div>
        <div className="toolbarActions">
          <button className="copyButton" title="Copy edited screenshot to clipboard" onClick={() => void copyEditedImage()}>
            {copyState === "copied" ? <ClipboardCheck size={17} /> : <Clipboard size={17} />}
            {copyState === "copied" ? "Скопировано" : "Буфер"}
          </button>
          <button
            className="saveButton"
            onClick={async () => {
              const saved = await window.multisnap.saveImage(exportImage());
              if (saved) await onSaved();
            }}
          >
            <Save size={17} />
            Save
          </button>
          <button className="iconButton" title="Close editor" onClick={onClose}>
            <X size={17} />
          </button>
        </div>
      </footer>
    </div>
  );
}

function ToolButton({
  active,
  children,
  label,
  onClick
}: {
  active: boolean;
  children: React.ReactNode;
  label: string;
  onClick: () => void;
}) {
  return (
    <button className={active ? "iconButton active" : "iconButton"} title={label} onClick={onClick}>
      {children}
    </button>
  );
}

function renderStroke(ctx: CanvasRenderingContext2D, stroke: Stroke) {
  ctx.save();
  ctx.strokeStyle = stroke.color;
  ctx.fillStyle = stroke.color;
  ctx.lineWidth = stroke.width;
  ctx.lineCap = "round";
  ctx.lineJoin = "round";

  if (stroke.tool === "pen") {
    ctx.beginPath();
    stroke.points.forEach((point, index) => (index === 0 ? ctx.moveTo(point.x, point.y) : ctx.lineTo(point.x, point.y)));
    ctx.stroke();
  }

  if (stroke.tool === "rect" || stroke.tool === "blur") {
    const first = stroke.points[0];
    const last = stroke.points[stroke.points.length - 1];
    const left = Math.min(first.x, last.x);
    const top = Math.min(first.y, last.y);
    const width = Math.abs(first.x - last.x);
    const height = Math.abs(first.y - last.y);
    if (stroke.tool === "blur") {
      ctx.globalAlpha = 0.82;
      ctx.fillStyle = "#d8dde8";
      ctx.fillRect(left, top, width, height);
    } else {
      ctx.strokeRect(left, top, width, height);
    }
  }

  if (stroke.tool === "arrow") {
    const first = stroke.points[0];
    const last = stroke.points[stroke.points.length - 1];
    drawArrow(ctx, first, last, stroke.width);
  }

  if (stroke.tool === "text" && stroke.text) {
    const point = stroke.points[0];
    ctx.font = `${Math.max(18, stroke.width * 5)}px Inter, Segoe UI, sans-serif`;
    ctx.fillText(stroke.text, point.x, point.y);
  }

  ctx.restore();
}

function drawArrow(ctx: CanvasRenderingContext2D, start: Point, end: Point, width: number) {
  const angle = Math.atan2(end.y - start.y, end.x - start.x);
  const head = Math.max(14, width * 4);
  ctx.beginPath();
  ctx.moveTo(start.x, start.y);
  ctx.lineTo(end.x, end.y);
  ctx.stroke();
  ctx.beginPath();
  ctx.moveTo(end.x, end.y);
  ctx.lineTo(end.x - head * Math.cos(angle - Math.PI / 6), end.y - head * Math.sin(angle - Math.PI / 6));
  ctx.lineTo(end.x - head * Math.cos(angle + Math.PI / 6), end.y - head * Math.sin(angle + Math.PI / 6));
  ctx.closePath();
  ctx.fill();
}
