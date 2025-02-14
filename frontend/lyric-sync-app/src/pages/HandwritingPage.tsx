import React from "react";
import HandwritingCanvas from "../components/HandwritingCanvas";
import { Container } from "@mui/material";

const HandwritingPage: React.FC = () => {
  return (
    <Container>
      <HandwritingCanvas />
    </Container>
  );
};

export default HandwritingPage;