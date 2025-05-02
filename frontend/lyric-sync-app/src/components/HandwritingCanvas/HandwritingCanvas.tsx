import React, { useRef, useState, useEffect } from 'react';
import {
  Box,
  Button,
  Typography,
  Stack,
  Tooltip
} from '@mui/material';
import { LanguageCode } from '../../api';
import './HandwritingCanvas.scss';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit'; // Marker
import { CleaningServices } from '@mui/icons-material';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faEraser } from '@fortawesome/free-solid-svg-icons';

interface HandwritingCanvasProps {
  expectedText: string;
  selectedLanguage: LanguageCode;
  onComplete?: (_matchPercentage: number) => void;
}

const HandwritingCanvas = React.forwardRef<{ clearCanvas: () => void }, HandwritingCanvasProps>(
  (props, ref) => {
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
        if (!canvasRef.current || !ctxRef.current) return;
        ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
        setFeedbackMessage('');
        setRecognizedText('');
        setMatchPercentage(null);
      },
      getImageData: () => {
        if (!canvasRef.current) return null;

        const offscreenCanvas = document.createElement('canvas');
        const ctx = offscreenCanvas.getContext('2d');
        offscreenCanvas.width = canvasRef.current.width;
        offscreenCanvas.height = canvasRef.current.height;

        if (ctx) {
          ctx.fillStyle = '#FFFFFF';
          ctx.fillRect(0, 0, offscreenCanvas.width, offscreenCanvas.height);
          ctx.drawImage(canvasRef.current, 0, 0);
        }

        return offscreenCanvas.toDataURL('image/png').split(',')[1]; // base64
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

    return (
      <div className="handwriting-canvas-card">
        <Box className="canvas-container">
          <canvas
            ref={gridCanvasRef}
            width={700}
            height={350}
            className="grid-canvas"
          />
          <canvas
            ref={canvasRef}
            className="drawing-canvas"
            onPointerDown={startDrawing}
            onPointerMove={draw}
            onPointerUp={stopDrawing}
            onPointerOut={stopDrawing}
            style={{
              cursor: isEraser
                ? 'url("data:image/svg+xml;utf8,<svg xmlns=%27http://www.w3.org/2000/svg%27 width=%2732%27 height=%2732%27 viewBox=%270 0 32 32%27><rect x=%279%27 y=%279%27 width=%2714%27 height=%2714%27 fill=%27grey%27/></svg>") 16 16, auto'
                : 'url("data:image/svg+xml;utf8,<svg xmlns=%27http://www.w3.org/2000/svg%27 width=%2732%27 height=%2732%27 viewBox=%270 0 32 32%27><path d=%27M16 2 V6 M16 26 V30 M2 16 H6 M26 16 H30%27 stroke=%27black%27 stroke-width=%272%27/><circle cx=%2716%27 cy=%2716%27 r=%274%27 fill=%27none%27 stroke=%27black%27 stroke-width=%271%27/></svg>") 16 16, crosshair'
            }}
          />

        </Box>

        <Stack direction="row" spacing={2} justifyContent="center" mt={2}>
          <Tooltip title="Draw (Pen Tool)">
            <Button
              variant={isEraser ? 'outlined' : 'contained'}
              color="secondary"
              onClick={() => setIsEraser(false)}
            >
              <EditIcon />
            </Button>
          </Tooltip>

          <Tooltip title="Erase">
            <Button
              variant={isEraser ? 'contained' : 'outlined'}
              color="secondary"
              onClick={() => setIsEraser(true)}
            >
              <FontAwesomeIcon icon={faEraser} />
            </Button>
          </Tooltip>

          <Tooltip title="Clear Canvas">
            <Button
              variant="outlined"
              color="secondary"
              onClick={clearCanvas}
            >
              <DeleteIcon />
            </Button>
          </Tooltip>
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