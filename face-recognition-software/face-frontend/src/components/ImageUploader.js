import React, { useRef } from "react";

function ImageUploader({ setImageUrl }) {
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

  return (
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
  );
}

export default ImageUploader;
