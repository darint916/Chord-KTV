import React, { useRef, useState, useEffect } from 'react';
import { Box, Button, Card, CardContent, Typography, Stack, TextField, MenuItem, Select, InputLabel, FormControl } from '@mui/material';
import { HandwritingApi, Configuration, LanguageCode } from '../api';

// Initialize API client
const handwritingApi = new HandwritingApi(
  new Configuration({
    basePath: import.meta.env.VITE_API_URL || 'http://localhost:5259',
  })
);

const HandwritingCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const ctxRef = useRef<CanvasRenderingContext2D | null>(null);
  const gridCanvasRef = useRef<HTMLCanvasElement | null>(null);
  const [isDrawing, setIsDrawing] = useState(false);
  const [isEraser, setIsEraser] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState('');
  const [recognizedText, setRecognizedText] = useState('');
  const [matchPercentage, setMatchPercentage] = useState<number | null>(null);
  const [expectedText, setExpectedText] = useState<string>('');
  const [selectedLanguage, setSelectedLanguage] = useState<LanguageCode>(LanguageCode.En);  // Default language is English

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
        drawGridlines(ctx, gridCanvas.width, gridCanvas.height);
      }
    }
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
    if (!ctxRef.current) {return;}
    const { offsetX, offsetY } = e.nativeEvent;
    ctxRef.current.beginPath();
    ctxRef.current.moveTo(offsetX, offsetY);
    setIsDrawing(true);
  };

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

  const stopDrawing = () => {
    if (!ctxRef.current) {return;}
    ctxRef.current.closePath();
    setIsDrawing(false);
  };

  const clearCanvas = () => {
    if (!canvasRef.current || !ctxRef.current) {return;}
    ctxRef.current.clearRect(0, 0, canvasRef.current.width, canvasRef.current.height);
    setFeedbackMessage('');
    setRecognizedText('');
    setMatchPercentage(null);
  };

  const exportImage = async () => {
    if (!canvasRef.current) {return;}
    const originalCanvas = canvasRef.current;
    const offscreenCanvas = document.createElement('canvas');
    const ctx = offscreenCanvas.getContext('2d');
    offscreenCanvas.width = originalCanvas.width;
    offscreenCanvas.height = originalCanvas.height;
    if (ctx) {
      ctx.fillStyle = '#FFFFFF';
      ctx.fillRect(0, 0, offscreenCanvas.width, offscreenCanvas.height);
      ctx.drawImage(originalCanvas, 0, 0);
    }

    const imageData = offscreenCanvas.toDataURL('image/png');

    try {
      const response = await handwritingApi.apiHandwritingOcrPost({
        handwritingCanvasRequestDto: {
          image: imageData.split(',')[1],
          language: selectedLanguage, // Send selected language
          expectedMatch: expectedText, // Send expected text
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
    <Card sx={{ maxWidth: 500, margin: 'auto', padding: 2, textAlign: 'center', boxShadow: 3 }}>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Handwriting Recognition
        </Typography>

        <Stack direction="row" spacing={2} justifyContent="center" mt={2} mb={2}>
          <TextField
            label="Expected Text"
            variant="outlined"
            value={expectedText}
            onChange={(e) => setExpectedText(e.target.value)}
          />
          <FormControl variant="outlined" sx={{ minWidth: 120 }}>
            <InputLabel>Language</InputLabel>
            <Select
              value={selectedLanguage}
              onChange={(e) => setSelectedLanguage(e.target.value as LanguageCode)}
              label="Language"
            >
              {Object.keys(LanguageCode).map((key) => (
                <MenuItem key={key} value={LanguageCode[key as keyof typeof LanguageCode]}>
                  {LanguageCode[key as keyof typeof LanguageCode]}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>

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
          <canvas
            ref={gridCanvasRef}
            width={500}
            height={300}
            style={{
              position: 'absolute',
              top: 0,
              left: 0,
              pointerEvents: 'none',
            }}
          />
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
            variant={isEraser ? 'contained' : 'outlined'}
            color="secondary"
            onClick={() => setIsEraser(prev => !prev)}
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
