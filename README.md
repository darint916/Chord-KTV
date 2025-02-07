# 🎶 Chord KTV 🎼🎤🎵
![Build Status](https://github.com/darint916/Chord-KTV/actions/workflows/dotnet-build-run-check.yml/badge.svg) ![Style Police Status](https://github.com/darint916/Chord-KTV/actions/workflows/enforce-code-style.yml/badge.svg)

## Overview 
Chord KTV is a web-based application designed to enhance language learning through music. 🎵 By synchronizing lyrics with audio playback, the platform provides users with native text, transliterations, and contextual English translations, facilitating an immersive and engaging learning experience. 🎼🎤🎶

## Features 🗣️
- **Synchronized Lyrics Display:** Real-time presentation of lyrics in both native script and transliterated form, accompanied by English translations.
- **Multiple Playback Options:** Choose between YouTube music videos or audio-only playback to suit your learning preference.
- **Interactive Quizzes:** Test your understanding with quizzes based on song segments, reinforcing language comprehension.
- **Handwriting Practice:** For languages with non-Latin scripts, practice writing characters using an integrated canvas-based tool.
- **Progress Tracking:** Monitor your learning journey with personalized progress reports, highlighting areas of improvement.

## Technical Details 🔤 
Chord KTV leverages **LRC files** to synchronize lyrics with music playback. 🎶 These files contain **time-stamped lyrics**, enabling a karaoke-like experience. 🎤 The application integrates with external APIs, including **YouTube and Spotify**, to manage song playback and playlists. 🎵 Additionally, the **ChatGPT API** is utilized to generate transliterations and translations, ensuring consistency and accuracy. 🎶🎼🎧

## Installation 🎵💻📥
### Clone the Repository: 🖥️📂🔽
```bash
git clone https://github.com/darint916/Chord-KTV.git
cd Chord-KTV
```

### Install Dependencies: ⚙️📦🔧
Ensure you have [**.NET SDK**](https://dotnet.microsoft.com/download) installed. Then, restore the required packages:
```bash
dotnet restore
```

### Run the Application: 🚀🖧🎛️
```bash
dotnet run
```

## Usage 🎷🎹⏯️
- **Select a Song:** Choose a song from the available library or import your own.
- **Follow Along:** As the song plays, follow the synchronized lyrics displayed on the screen.
- **Take Quizzes:** After listening, engage in quizzes to test your comprehension.
- **Practice Writing:** Utilize the handwriting feature to practice writing characters for applicable languages.

## Contributing 🌍🤝🎼
We welcome contributions from the community! To get involved:

1. **Fork the Repository:** Click the "Fork" button at the top right of this page.
2. **Create a Branch:** Use a descriptive name for your branch.
3. **Make Changes:** Implement your feature or fix.
4. **Submit a Pull Request:** Provide a clear description of your changes. (Use the PR template)

## License 📜📝⚖️
This project is licensed under the [**MIT License**](LICENSE). 🎼🎵🎧
