import React, { useRef, useState, useEffect } from 'react';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Stack,
} from '@mui/material';
import { HandwritingApi, Configuration, LanguageCode } from '../../api';
import './HandwritingCanvas.scss';

// Initialize API client
const handwritingApi = new HandwritingApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
  })
);

interface HandwritingCanvasProps {
  expectedText: string;
  selectedLanguage: LanguageCode;
}

const HandwritingCanvas: React.FC<HandwritingCanvasProps> = ({ expectedText, selectedLanguage }) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
  const gridCanvasRef = useRef<HTMLCanvasElement | null>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [isEraser, setIsEraser] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState('');
  const [recognizedText, setRecognizedText] = useState('');
  const [matchPercentage, setMatchPercentage] = useState<number | null>(null);

  // Initialize canvas and grid
  useEffect(() => {
    const initializeCanvas = () => {
      if (canvasRef.current) {
        const ctx = canvasRef.current.getContext('2d');
        if (ctx) {
          ctx.lineCap = 'round';
          ctx.lineJoin = 'round';
          ctx.lineWidth = 4;
          ctx.strokeStyle = '#000';
          ctxRef.current = ctx;
        }
      }

      if (gridCanvasRef.current) {
        const ctx = gridCanvasRef.current.getContext('2d');
        if (ctx) {
          drawGridlines(ctx, gridCanvasRef.current.width, gridCanvasRef.current.height);
        }
      }
    };

    initializeCanvas();
  }, []);

  // Draw gridlines on the canvas
  const drawGridlines = (ctx: CanvasRenderingContext2D, width: number, height: number) => {
    ctx.clearRect(0, 0, width, height);
    ctx.strokeStyle = '#e0e0e0';
    ctx.lineWidth = 0.5;
    const gridSize = 50;

    for (let x = gridSize; x < width; x += gridSize) {
      ctx.beginPath();
      ctx.moveTo(x, 0);
      ctx.lineTo(x, height);
      ctx.stroke();
    }

    for (let y = gridSize; y < height; y += gridSize) {
      ctx.beginPath();
      ctx.moveTo(0, y);
      ctx.lineTo(width, y);
      ctx.stroke();
    }
  };

  // Handle drawing start
  const startDrawing = (e: React.PointerEvent) => {
    if (!ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.beginPath();
    ctxRef.current.moveTo(offsetX, offsetY);
    setIsDrawing(true);
  };

  // Handle drawing
  const draw = (e: React.PointerEvent) => {
    if (!isDrawing || !ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;

    if (isEraser) {
      ctxRef.current.clearRect(offsetX - 10, offsetY - 10, 20, 20);
    } else {
      ctxRef.current.lineTo(offsetX, offsetY);
      ctxRef.current.stroke();
    }
  };

  // Handle drawing stop
  const stopDrawing = () => {
    if (!ctxRef.current) {return;}
    ctxRef.current.closePath();
    setIsDrawing(false);
  };

  // Clear the canvas
  const clearCanvas = () => {
    if (!canvasRef.current || !ctxRef.current) {return;}
    ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
    setFeedbackMessage('');
    setRecognizedText('');
    setMatchPercentage(null);
  };

  // Export the canvas image and send it for recognition
  const exportImage = async () => {
    if (!canvasRef.current) {return;}

    const offscreenCanvas = document.createElement('canvas');
    const ctx = offscreenCanvas.getContext('2d');
    offscreenCanvas.width = canvasRef.current.width;
    offscreenCanvas.height = canvasRef.current.height;

    if (ctx) {
      ctx.fillStyle = '#FFFFFF';
      ctx.fillRect(0, 0, offscreenCanvas.width, offscreenCanvas.height);
      ctx.drawImage(canvasRef.current, 0, 0);
    }

    const imageData = offscreenCanvas.toDataURL('image/png');

    try {
      const response = await handwritingApi.apiHandwritingOcrPost({
        handwritingCanvasRequestDto: {
          image: imageData.split(',')[1],
          language: selectedLanguage,
          expectedMatch: expectedText,
        },
      });

      const match = response.matchPercentage || 0;
      const recognizedText = response.recognizedText || '';
      setRecognizedText(recognizedText);
      setMatchPercentage(match);

      if (match === 100) {
        setFeedbackMessage('Good job!');
      } else {
        setFeedbackMessage(`Try again! Match: ${match}%`);
      }
    } catch {
      setFeedbackMessage('Error in recognition. Please try again.');
    }
  };

  return (
    <Card className="handwriting-canvas-card">
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Handwriting Recognition
        </Typography>

        <Box className="canvas-container">
          <canvas
            ref={gridCanvasRef}
            width={500}
            height={300}
            className="grid-canvas"
          />
          <canvas
            ref={canvasRef}
            width={500}
            height={300}
            className="drawing-canvas"
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
            variant={isEraser ? 'contained' : 'outlined'}
            color="secondary"
            onClick={() => setIsEraser((prev) => !prev)}
          >
            {isEraser ? 'Drawing' : 'Erase'}
          </Button>
        </Stack>

        <Typography variant="h6" mt={2}>
          {feedbackMessage}
        </Typography>

        {recognizedText && (
          <Typography variant="body1" mt={1}>
            Recognized Text: {recognizedText}
          </Typography>
        )}

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
