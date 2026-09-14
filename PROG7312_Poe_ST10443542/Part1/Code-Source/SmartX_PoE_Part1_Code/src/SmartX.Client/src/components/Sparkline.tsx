interface SparklineProps {
  values: number[];
  width?: number;
  height?: number;
  stroke?: string;
}

/** Minimal inline-SVG sparkline — part of the real-time visual feedback engagement feature. */
export function Sparkline({ values, width = 160, height = 36, stroke = "#4c8dff" }: SparklineProps) {
  if (values.length < 2) {
    return <svg width={width} height={height} />;
  }

  const min = Math.min(...values);
  const max = Math.max(...values);
  const range = max - min || 1;

  const points = values
    .map((v, i) => {
      const x = (i / (values.length - 1)) * (width - 4) + 2;
      const y = height - 2 - ((v - min) / range) * (height - 4);
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(" ");

  return (
    <svg width={width} height={height} role="img" aria-label="Recent value trend">
      <polyline points={points} fill="none" stroke={stroke} strokeWidth={1.75} strokeLinejoin="round" strokeLinecap="round" />
    </svg>
  );
}
