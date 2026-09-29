import React, { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { promptApi } from '../../services/api/promptApi';
import './VisionPrompt.css';

const VisionPrompt: React.FC = () => {
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [base64Image, setBase64Image] = useState<string | null>(null);
  const [generatedPrompt, setGeneratedPrompt] = useState<string>('');
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [error, setError] = useState<string>('');

  const onDrop = useCallback((acceptedFiles: File[]) => {
    setError('');
    setGeneratedPrompt('');
    const file = acceptedFiles[0];
    if (file) {
      const previewUrl = URL.createObjectURL(file);
      setImagePreview(previewUrl);

      const reader = new FileReader();
      reader.onloadend = () => {
        setBase64Image(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'image/*': [] },
    multiple: false
  });

  const handleAnalyze = async () => {
    if (!base64Image) return;

    setIsAnalyzing(true);
    setError('');
    
    try {
      const response = await promptApi.analyzeImage(base64Image);
      setGeneratedPrompt(response.result);
    } catch (err) {
      setError('Failed to analyze the image. Please try again.');
    } finally {
      setIsAnalyzing(false);
    }
  };

  return (
    <div className="vision-prompt-container">
      <div className="vision-header">
        <h2>Vision-to-Prompt AI</h2>
        <p>Upload an image and let AI reverse-engineer it into a highly detailed text prompt.</p>
      </div>

      <div className="vision-panels">
        {/* Left Panel: Image Upload */}
        <div className="vision-panel left-panel">
          <div 
            {...getRootProps()} 
            className={`dropzone ${isDragActive ? 'active' : ''} ${imagePreview ? 'has-image' : ''}`}
          >
            <input {...getInputProps()} />
            {imagePreview ? (
              <img src={imagePreview} alt="Preview" className="image-preview" />
            ) : (
              <div className="dropzone-text">
                <p>Drag & drop an image here, or click to select</p>
                <span>Supports JPG, PNG, WEBP</span>
              </div>
            )}
          </div>

          <div className="panel-actions">
            <button 
              className="analyze-btn"
              onClick={handleAnalyze} 
              disabled={!base64Image || isAnalyzing}
            >
              {isAnalyzing ? 'Analyzing Image...' : 'Reverse Engineer Prompt'}
            </button>
            {error && <div className="error-message">{error}</div>}
          </div>
        </div>

        {/* Right Panel: Generated Prompt */}
        <div className="vision-panel right-panel">
          <h3>A detailed description for AI image generation:</h3>
          <div className="generated-prompt-box">
            {isAnalyzing ? (
              <div className="loading-state">
                <div className="spinner"></div>
                <p>Analyzing style, lighting, and composition...</p>
              </div>
            ) : generatedPrompt ? (
              <p className="prompt-text">{generatedPrompt}</p>
            ) : (
              <p className="empty-text">Upload and analyze an image to see the prompt here.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default VisionPrompt;
