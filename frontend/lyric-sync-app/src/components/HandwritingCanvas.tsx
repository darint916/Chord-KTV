import React, { useRef, useState } from "react";
import { Box, Button, Card, CardContent, Typography, Stack } from "@mui/material";

const HandwritingCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
  const [isDrawing, setIsDrawing] = useState(false);

  // Initialize canvas settings
  React.useEffect(() => {
    if (canvasRef.current) {
      const canvas = canvasRef.current;
      const ctx = canvas.getContext("2d");

      if (ctx) {
        ctx.lineCap = "round";
        ctx.lineJoin = "round";
        ctx.lineWidth = 4;
        ctx.strokeStyle = "#000";
        ctxRef.current = ctx;
      }
    }
  }, []);

  // Start drawing
  const startDrawing = (e: React.PointerEvent) => {
    if (!ctxRef.current) return;
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.beginPath();
    ctxRef.current.moveTo(offsetX, offsetY);
    setIsDrawing(true);
  };

  // Draw on canvas
  const draw = (e: React.PointerEvent) => {
    if (!isDrawing || !ctxRef.current) return;
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.lineTo(offsetX, offsetY);
    ctxRef.current.stroke();
  };

  // Stop drawing
  const stopDrawing = () => {
    if (!ctxRef.current) return;
    ctxRef.current.closePath();
    setIsDrawing(false);
  };

  // Clear canvas
  const clearCanvas = () => {
    if (!canvasRef.current || !ctxRef.current) return;
    ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
  };

  // Export image
  const exportImage = () => {
    // TODO: Add call to backend to save the image/evaluation endpoint
    // if (!canvasRef.current) return;
    // const imageData = canvasRef.current.toDataURL("image/png");
  };

  return (
    <Card sx={{ maxWidth: 500, margin: "auto", padding: 2, textAlign: "center", boxShadow: 3 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Handwriting Recognition
        </Typography>

        <Box
          sx={{
            border: "2px solid #1976d2",
            borderRadius: "8px",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            background: "#f9f9f9",
            overflow: "hidden",
            touchAction: "none",
          }}
        >
          <canvas
            ref={canvasRef}
            width={500}
            height={300}
            style={{ cursor: "crosshair", backgroundColor: "white" }}
            onPointerDown={startDrawing}
            onPointerMove={draw}
            onPointerUp={stopDrawing}
            onPointerOut={stopDrawing}
          />
        </Box>

        <Stack direction="row" spacing={2} justifyContent="center" mt={2}>
          <Button variant="contained" color="primary" onClick={exportImage}>
            Recognize
          </Button>
          <Button variant="outlined" color="secondary" onClick={clearCanvas}>
            Clear
          </Button>
        </Stack>
      </CardContent>
    </Card>
  );
};

export default HandwritingCanvas;