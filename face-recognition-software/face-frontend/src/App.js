import React, { useState, useEffect, useRef } from "react";
import './App.css';

function App() {
  const [isModelLoading, setIsModelLoading] = useState(false);
  const [imageUrl, setImageUrl] = useState(null);
  const [result, setResult] = useState('');
  const [details, setDetails] = useState('');

  const imageRef = useRef();
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

  useEffect(() => {
    setIsModelLoading(true);
    // Simulating model load (if needed in future)
    setIsModelLoading(false);
  }, []);

  const handleImageRecognition = async () => {
    const formData = new FormData();
    formData.append('image', fileInputRef.current.files[0]);

    try {
        const response = await fetch('http://localhost:5000/api/ImageRecognition/upload', {
            method: 'POST',
            body: formData,
        });

        const data = await response.json();
        console.log(data);  // Debug line to inspect the response

        setResult(data.message);
        setDetails(`Time: ${data.time}`);
        if (data.name) {
            setDetails(prevDetails => `${prevDetails}, Name: ${data.name}`);
        }
    } catch (error) {
        console.error('Error:', error);
        setResult('An error occurred. Please try again.');
        setDetails('');
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
        />
        <button className="uploadImage" onClick={uploadTrigger}>
          Upload Image
        </button>
      </div>
      <div className="imageWrapper">
        <div className="imageContent">
          <div className="imageArea">
            {imageUrl && (
              <img
                src={imageUrl}
                alt="Image Preview"
                crossOrigin="anonymous"
                ref={imageRef}
              />
            )}
          </div>
        </div>
        {imageUrl && (
          <button className="button" onClick={handleImageRecognition}>
            Check Employee
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
