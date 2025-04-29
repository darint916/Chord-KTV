import React, { useRef, useState, useEffect } from 'react';
import {
  Box,
  Button,
  Typography,
  Stack,
} from '@mui/material';
import { LanguageCode } from '../../api';
import { handwritingApi } from '../../api/apiClient';
import './HandwritingCanvas.scss';

interface HandwritingCanvasProps {
  expectedText: string;
  selectedLanguage: LanguageCode;
  onComplete?: (_matchPercentage: number) => void;
}

const HandwritingCanvas = React.forwardRef<{ clearCanvas: () => void }, HandwritingCanvasProps>(
  (props, ref) => {
    const { expectedText, selectedLanguage, onComplete } = props;
    const canvasRef = useRef<HTMLCanvasElement | null>(null);
    const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
    const gridCanvasRef = useRef<HTMLCanvasElement | null>(null);
    const [isDrawing, setIsDrawing] = useState(false);
    const [isEraser, setIsEraser] = useState(false);
    const [feedbackMessage, setFeedbackMessage] = useState('');
    const [recognizedText, setRecognizedText] = useState('');
    const [matchPercentage, setMatchPercentage] = useState<number | null>(null);

    React.useImperativeHandle(ref, () => ({
      clearCanvas: () => {
        if (!canvasRef.current || !ctxRef.current) {return;}
        ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
        setFeedbackMessage('');
        setRecognizedText('');
        setMatchPercentage(null);
      }
    }));

    useEffect(() => {
      const initializeCanvas = () => {
        if (canvasRef.current && gridCanvasRef.current) {
          // Get the container dimensions
          const container = canvasRef.current.parentElement;
          if (container) {
            const width = container.clientWidth;
            const height = container.clientHeight;
            
            // Set canvas dimensions to match display
            canvasRef.current.width = width;
            canvasRef.current.height = height;
            gridCanvasRef.current.width = width;
            gridCanvasRef.current.height = height;
            
            const ctx = canvasRef.current.getContext('2d');
            if (ctx) {
              ctx.lineCap = 'round';
              ctx.lineJoin = 'round';
              ctx.lineWidth = 4;
              ctx.strokeStyle = '#000';
              ctxRef.current = ctx;
            }

            const gridCtx = gridCanvasRef.current.getContext('2d');
            if (gridCtx) {
              drawGridlines(gridCtx, width, height);
            }
          }
        }
      };

      initializeCanvas();

      // Add resize listener
      const handleResize = () => initializeCanvas();
      window.addEventListener('resize', handleResize);
      return () => window.removeEventListener('resize', handleResize);
    }, []);

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

    const startDrawing = (e: React.PointerEvent) => {
      if (!ctxRef.current) { return; }
      const { offsetX, offsetY } = e.nativeEvent;
      ctxRef.current.beginPath();
      ctxRef.current.moveTo(offsetX, offsetY);
      setIsDrawing(true);
    };

    const draw = (e: React.PointerEvent) => {
      if (!isDrawing || !ctxRef.current) { return; }
      const { offsetX, offsetY } = e.nativeEvent;

      if (isEraser) {
        ctxRef.current.clearRect(offsetX - 10, offsetY - 10, 20, 20);
      } else {
        ctxRef.current.lineTo(offsetX, offsetY);
        ctxRef.current.stroke();
      }
    };

    const stopDrawing = () => {
      if (!ctxRef.current) { return; }
      ctxRef.current.closePath();
      setIsDrawing(false);
    };

    const clearCanvas = () => {
      if (!canvasRef.current || !ctxRef.current) { return; }
      ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
      setFeedbackMessage('');
      setRecognizedText('');
      setMatchPercentage(null);
    };

    const exportImage = async () => {
      if (!canvasRef.current) { return; }

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
          if (onComplete) { onComplete(100); }
        } else {
          setFeedbackMessage(`Try again! Match: ${match}%`);
          if (onComplete) { onComplete(match); }
        }
      } catch {
        setFeedbackMessage('Error in recognition. Please try again.');
        if (onComplete) { onComplete(-1); }
      }
    };

    return (
      <div className="handwriting-canvas-card">
        <Box className="canvas-container">
          <canvas
            ref={gridCanvasRef}
            width={500}
            height={300}
            className="grid-canvas"
          />
          <canvas
            ref={canvasRef}
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
            {isEraser ? 'Draw' : 'Erase'}
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
      </div>
    );
  }
);

HandwritingCanvas.displayName = 'HandwritingCanvas';

export default HandwritingCanvas;