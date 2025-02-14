import React from "react";
import HandwritingCanvas from "../components/HandwritingCanvas";
import { Container, Box } from "@mui/material";

const HandwritingPage: React.FC = () => {
    return (
        <Container maxWidth="md">
        <Box
            sx={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100vh", // Full viewport height
            }}
        >
            <HandwritingCanvas />
        </Box>
        </Container>
    );
};

export default HandwritingPage;