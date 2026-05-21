import { useEffect, useMemo, useRef, useState } from "react";
import {
  ArrowUpRight,
  Clipboard,
  Crop,
  Download,
  Eraser,
  FolderOpen,
  Image as ImageIcon,
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
import type { HistoryItem } from "./electron";

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
  const isOverlay = window.location.hash === "#/overlay";
  return isOverlay ? <OverlayCapture /> : <MainApp />;
}

function MainApp() {
  const [history, setHistory] = useState<HistoryItem[]>([]);
  const [image, setImage] = useState<string | null>(null);

  const refreshHistory = async () => {
    setHistory(await window.multisnap.getHistory());
  };

  useEffect(() => {
    void refreshHistory();
    const offImage = window.multisnap.onEditorImage((dataUrl) => setImage(dataUrl));
    const offHistory = window.multisnap.onHistoryChanged(() => void refreshHistory());
    return () => {
      offImage();
      offHistory();
    };
  }, []);

  return (
    <main className="appShell">
      <aside className="sidePanel">
        <div className="brandBlock">
          <div className="brandMark">M</div>
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
            <p>Use the buttons on the left or press Ctrl+Shift+5 for area capture.</p>
          </div>
        )}
      </section>
    </main>
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

  return (
    <div
      className="overlayRoot"
      onMouseDown={(event) => {
        setStart({ x: event.clientX, y: event.clientY });
        setCurrent({ x: event.clientX, y: event.clientY });
      }}
      onMouseMove={(event) => {
        if (start) setCurrent({ x: event.clientX, y: event.clientY });
      }}
      onMouseUp={() => void cropSelection()}
    >
      {screenshot && <img ref={imageRef} src={screenshot} className="overlayShot" draggable={false} />}
      <div className="overlayShade" />
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
      <div className="overlayHint">Drag to select area. Esc cancels.</div>
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
  const [tool, setTool] = useState<Tool>("pen");
  const [color, setColor] = useState(colors[0]);
  const [width, setWidth] = useState(4);
  const [strokes, setStrokes] = useState<Stroke[]>([]);
  const [draft, setDraft] = useState<Stroke | null>(null);

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

  const canvasPoint = (event: React.PointerEvent<HTMLCanvasElement>): Point => {
    const canvas = canvasRef.current!;
    const box = canvas.getBoundingClientRect();
    return {
      x: ((event.clientX - box.left) / box.width) * canvas.width,
      y: ((event.clientY - box.top) / box.height) * canvas.height
    };
  };

  const exportImage = () => canvasRef.current?.toDataURL("image/png") ?? image;

  return (
    <div className="editorShell">
      <header className="toolbar">
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
        <div className="spacer" />
        <button className="iconButton" title="Undo" onClick={() => setStrokes((items) => items.slice(0, -1))}>
          <Undo2 size={17} />
        </button>
        <button className="iconButton" title="Copy" onClick={() => window.multisnap.copyImage(exportImage())}>
          <Clipboard size={17} />
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
