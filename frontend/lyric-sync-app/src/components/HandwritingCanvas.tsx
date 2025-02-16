import React, { useRef, useState, useEffect } from 'react';
import { Box, Button, Card, CardContent, Typography, Stack } from '@mui/material';

const HandwritingCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
  const gridCanvasRef = useRef<HTMLCanvasElement | null>(null); // Grid canvas
  const [isDrawing, setIsDrawing] = useState(false);

  // Initialize canvas settings
  useEffect(() => {
    if (canvasRef.current) {
      const canvas = canvasRef.current;
      const ctx = canvas.getContext('2d');

      if (ctx) {
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.lineWidth = 4;
        ctx.strokeStyle = '#000';
        ctxRef.current = ctx;
      }
    }

    if (gridCanvasRef.current) {
      const gridCanvas = gridCanvasRef.current;
      const ctx = gridCanvas.getContext('2d');
      if (ctx) {
        // Draw gridlines on the grid canvas
        drawGridlines(ctx, gridCanvas.width, gridCanvas.height);
      }
    }
  }, []);

  // Draw gridlines function
  const drawGridlines = (ctx: CanvasRenderingContext2D, width: number, height: number) => {
    ctx.clearRect(0, 0, width, height); // Clear previous gridlines
    ctx.strokeStyle = '#e0e0e0'; // Light grey gridlines
    ctx.lineWidth = 0.5;
    const gridSize = 50; // Size of each grid square

    // Draw vertical gridlines
    for (let x = gridSize; x < width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, height);
      ctx.stroke();
    }

    // Draw horizontal gridlines
    for (let y = gridSize; y < height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(width, y);
      ctx.stroke();
    }
  };

  // Start drawing
  const startDrawing = (e: React.PointerEvent) => {
    if (!ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.beginPath();
    ctxRef.current.moveTo(offsetX, offsetY);
    setIsDrawing(true);
  };

  // Draw on canvas
  const draw = (e: React.PointerEvent) => {
    if (!isDrawing || !ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.lineTo(offsetX, offsetY);
    ctxRef.current.stroke();
  };

  // Stop drawing
  const stopDrawing = () => {
    if (!ctxRef.current) {return;}
    ctxRef.current.closePath();
    setIsDrawing(false);
  };

  // Clear canvas
  const clearCanvas = () => {
    if (!canvasRef.current || !ctxRef.current) {return;}
    ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
  };

  // Export image
  const exportImage = () => {
    // if (!canvasRef.current) {return;}

    // // Create a new image data URL without the gridlines
    // const imageData = canvasRef.current.toDataURL('image/png');

    // TODO: Send the imageData to the backend or handle it as needed, delete this log call after
    // console.log(imageData);
  };

  return (
    <Card sx={{ maxWidth: 500, margin: 'auto', padding: 2, textAlign: 'center', boxShadow: 3 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Handwriting Recognition
        </Typography>

        <Box
          sx={{
            border: '2px solid #1976d2',
            borderRadius: '8px',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            background: '#f9f9f9',
            overflow: 'hidden',
            touchAction: 'none',
            position: 'relative',
          }}
        >
          {/* Grid canvas */}
          <canvas
            ref={gridCanvasRef}
            width={500}
            height={300}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              pointerEvents: 'none', // Disable interaction with the grid canvas
            }}
          />

          {/* Drawing canvas */}
          <canvas
            ref={canvasRef}
            width={500}
            height={300}
            style={{ cursor: 'crosshair', backgroundColor: 'white' }}
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
