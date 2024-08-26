import React from "react";

function ResultDisplay({ result, details }) {
  return (
    <div className="resultMessage">
      {result && <p>{result}</p>}
      {details && <p>{details}</p>}
    </div>
  );
}

export default ResultDisplay;
