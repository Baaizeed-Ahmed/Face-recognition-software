import React, { useState, useRef } from "react";
import './App.css';

function App() {
  const [imageUrl, setImageUrl] = useState(null);
  const [result, setResult] = useState('');
  const [details, setDetails] = useState('');
  const [isLoading, setIsLoading] = useState(false); // Track loading state

  const fileInputRef = useRef();

  const uploadImage = (e) => {
    const { files } = e.target;
    if (files.length > 0) {
      const url = URL.createObjectURL(files[0]);
      setImageUrl(url);
    } else {
      setImageUrl(null);
    }
  };

  const uploadTrigger = () => {
    fileInputRef.current.click();
  };

  const handleImageRecognition = async () => {
    if (!fileInputRef.current.files[0]) {
      setResult('No image selected.');
      return;
    }

    const formData = new FormData();
    formData.append('image', fileInputRef.current.files[0]);

    setIsLoading(true); // Start loading

    try {
        const response = await fetch('http://localhost:5217/api/ImageRecognition/upload', {
            method: 'POST',
            body: formData,
        });

        const data = await response.json();
        setResult(data.message);
        setDetails(`Time: ${data.time}`);
        if (data.name) {
            setDetails(prevDetails => `${prevDetails}, Name: ${data.name}`);
        }
    } catch (error) {
        console.error('Error:', error);
        setResult('An error occurred. Please try again.');
        setDetails('');
    } finally {
        setIsLoading(false); // End loading
    }
  };

  return (
    <div>
      <h1 className="header">Employee Recognition</h1>
      <div className="inputField">
        <input
          type="file"
          accept="image/*"
          capture="camera"
          className="uploadInput"
          onChange={uploadImage}
          ref={fileInputRef}
          style={{ display: 'none' }}
        />
        <button className="uploadImage" onClick={uploadTrigger}>
          Upload Image
        </button>
      </div>
      <div className="imageWrapper">
        <div className="imageContent">
          {imageUrl && (
            <div className="imageArea">
              <img
                src={imageUrl}
                alt="Image Preview"
                crossOrigin="anonymous"
              />
            </div>
          )}
        </div>
        {imageUrl && (
          <button className="button" onClick={handleImageRecognition} disabled={isLoading}>
            {isLoading ? 'Processing...' : 'Check Employee'}
          </button>
        )}
        {result && (
          <div className="resultMessage">
            <p>{result}</p>
            <p>{details}</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
