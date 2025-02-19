import React, { useRef, useState, useEffect } from 'react';
import { Box, Button, Card, CardContent, Typography, Stack } from '@mui/material';
import { HandwritingApi, Configuration } from '../api';

// Initialize API client
const handwritingApi = new HandwritingApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || "http://localhost:5259",
  })
);

const HandwritingCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
  const gridCanvasRef = useRef<HTMLCanvasElement | null>(null); // Grid canvas
  const [isDrawing, setIsDrawing] = useState(false);
  const [isEraser, setIsEraser] = useState(false); // Track eraser mode
  const [feedbackMessage, setFeedbackMessage] = useState(""); // Feedback message
  const [recognizedText, setRecognizedText] = useState(""); // Recognized text
  const [matchPercentage, setMatchPercentage] = useState<number | null>(null); // Match percentage

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

  // Draw or erase on canvas
  const draw = (e: React.PointerEvent) => {
    if (!isDrawing || !ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;

    if (isEraser) {
      // Eraser logic
      ctxRef.current.clearRect(offsetX - 10, offsetY - 10, 20, 20); // Clear a small area
    } else {
      // Drawing logic
      ctxRef.current.lineTo(offsetX, offsetY);
      ctxRef.current.stroke();
    }
  };

  // Stop drawing or erasing
  const stopDrawing = () => {
    if (!ctxRef.current) {return;}
    ctxRef.current.closePath();
    setIsDrawing(false);
  };

  // Clear canvas
  const clearCanvas = () => {
    if (!canvasRef.current || !ctxRef.current) {return;}
    ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
    setFeedbackMessage(""); // Clear feedback
    setRecognizedText(""); // Clear recognized text
    setMatchPercentage(null); // Clear match percentage
  };

  // Export image and get recognition result
  const exportImage = async () => {
    if (!canvasRef.current) return;

    // Get the existing canvas and its context
    const originalCanvas = canvasRef.current;

    // Create a new offscreen canvas
    const offscreenCanvas = document.createElement("canvas");
    const ctx = offscreenCanvas.getContext("2d");

    // Set the same dimensions as the original
    offscreenCanvas.width = originalCanvas.width;
    offscreenCanvas.height = originalCanvas.height;

    if (ctx) {
      // Fill the background with white
      ctx.fillStyle = "#FFFFFF";
      ctx.fillRect(0, 0, offscreenCanvas.width, offscreenCanvas.height);

      // Draw the original canvas content on top of the white background
      ctx.drawImage(originalCanvas, 0, 0);
    }

    // Create a new image data URL without the gridlines
    const imageData = offscreenCanvas.toDataURL("image/png");

    try {
      // Make API call
      const response = await handwritingApi.apiHandwritingOcrPost({
        handwritingCanvasRequestDto: {
          image: imageData.split(",")[1],
          language: "JA",
          expectedMatch: "こん", // Change based on your needs
        },
      });

      console.log("OCR Response:", response);

      // Handle feedback based on OCR result
      const match = response.matchPercentage || 0;
      const recognizedText = response.recognizedText || '';
      setRecognizedText(recognizedText);
      setMatchPercentage(match);

      if (match === 100) {
        setFeedbackMessage("Good job!");
      } else {
        setFeedbackMessage(`Try again! Match: ${match}%`);
      }
    } catch (error) {
      console.error("Error in OCR API call:", error);
      setFeedbackMessage("Error in recognition. Please try again.");
    }
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
          <Button
            variant={isEraser ? "contained" : "outlined"}
            color="secondary"
            onClick={() => setIsEraser(prev => !prev)}
          >
            {isEraser ? "Drawing" : "Erase"}
          </Button>
        </Stack>

        {/* Feedback Message */}
        <Typography variant="h6" mt={2}>
          {feedbackMessage}
        </Typography>

        {/* Recognized Text */}
        {recognizedText && (
          <Typography variant="body1" mt={1}>
            Recognized Text: {recognizedText}
          </Typography>
        )}

        {/* Match Percentage */}
        {matchPercentage !== null && (
          <Typography variant="body2" mt={1}>
            Match: {matchPercentage}%
          </Typography>
        )}
      </CardContent>
    </Card>
  );
};

export default HandwritingCanvas;
