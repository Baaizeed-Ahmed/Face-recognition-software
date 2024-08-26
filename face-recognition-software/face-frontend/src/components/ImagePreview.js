import React from "react";

function ImagePreview({ imageUrl, onCheckEmployee, isLoading }) {
  return (
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
        <button className="button" onClick={() => onCheckEmployee()} disabled={isLoading}>
          {isLoading ? 'Processing...' : 'Check Employee'}
        </button>
      )}
    </div>
  );
}

export default ImagePreview;
