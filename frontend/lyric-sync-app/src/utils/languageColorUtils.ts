/**  Language → colour (muted, well-separated, deterministic)  */
type LangColour = { bgcolor: string; color: string };

/* --- 1.  module-level lookup tables (persist between calls) --- */
const usedHues: number[] = [];                 // already assigned hues
const langHueMap = new Map<string, number>();  // language → hue

/* --- 2.  tunables ------------------------------------------------ */
const GOLDEN_ANGLE = 137.508;  // °  keeps successive hues “randomised”
const MIN_HUE_GAP  = 25;       // °  guarantee perceptual distance
const SATURATION   = 24;       // %  low saturation  →  muted pastel
const LIGHTNESS    = 48;       // %  mid-light so either black OR white text works

export function getLanguageColor(languageCode: string): LangColour {
  /* 2.1 normalise input */
  const code = languageCode.toUpperCase().trim();

  /* 2.2 quick return if colour was already issued */
  if (langHueMap.has(code)) {
    const hue = langHueMap.get(code);
    return hueToColour(hue ?? 0); // Provide a numeric fallback hue
  }

  /* 2.3 reproducible starting hue  (32-bit Fowler-Noll hash variant) */
  let hash = 2166136261;
  for (let i = 0; i < code.length; i++) {
    hash ^= code.charCodeAt(i);
    hash = (hash * 16777619) >>> 0;            // unsigned 32-bit
  }
  let hue = hash % 360;

  /* 2.4 collision–avoidance loop  (golden-angle jumps) */
  const limit = Math.floor(360 / MIN_HUE_GAP);
  for (let attempts = 0; attempts < limit; attempts++) {
    if (isHueDistinct(hue)) {break;}
    hue = (hue + GOLDEN_ANGLE) % 360;
  }

  /* 2.5 commit hue for this language */
  usedHues.push(hue);
  langHueMap.set(code, hue);

  return hueToColour(hue);
}

/* ---------- helpers ---------- */

/** true ⇢ no existing hue lies within MIN_HUE_GAP degrees */
function isHueDistinct(h: number): boolean {
  return usedHues.every(u => angularDistance(u, h) >= MIN_HUE_GAP);
}

/** shortest arc between two angles (0–180) */
const angularDistance = (a: number, b: number) =>
  Math.min(Math.abs(a - b), 360 - Math.abs(a - b));

/** HSL → CSS strings + readable text colour */
function hueToColour(h: number): LangColour {
  const bg = `hsl(${h.toFixed(0)}, ${SATURATION}%, ${LIGHTNESS}%)`;

  /* convert to RGB once to decide text contrast */
  const [r, g, b] = hslToRgb(h, SATURATION, LIGHTNESS);
  const luminance = 0.299 * r + 0.587 * g + 0.114 * b; // ITU-R BT.601
  const text = luminance > 0.55 ? 'rgba(0,0,0,0.87)' : '#ffffff';

  return { bgcolor: bg, color: text };
}

/*  HSL(0-360,%,%) → RGB(0-1) */
function hslToRgb(h: number, s: number, l: number): [number, number, number] {
  s /= 100; l /= 100; h /= 360;
  const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
  const p = 2 * l - q;

  const k = (n: number) => (h + n) % 1;
  const f = (n: number) => {
    const t = k(n);
    return p + (q - p) * (6 * t < 1 ? 6 * t :
      2 * t < 1 ? 1 :
        3 * t < 2 ? (2 / 3 - t) * 6 : 0);
  };
  return [f(1 / 3), f(0), f(-1 / 3)];
}
