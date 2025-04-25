import React from 'react';
import { Box, Chip, Paper, Typography, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { getLanguageColor } from '../../utils/languageColorUtils';

interface WordDto {
  word: string;
  language: string;
}

interface LearnedWordsProps {
  recent: WordDto[];
}

const LearnedWords: React.FC<LearnedWordsProps> = ({ recent }) => {
  // State for the selected language filter. An empty string represents "All"
  const [filterLanguage, setFilterLanguage] = React.useState<string>('');

  // Compute the unique languages from the recent words list, excluding "UNK"
  const uniqueLanguages = React.useMemo(() => {
    const languages = new Set(recent.map(word => word.language).filter(lang => lang !== 'UNK'));
    return Array.from(languages).sort();
  }, [recent]);

  // Filter words based on the selected language filter, excluding "UNK"
  const filteredWords = React.useMemo(() => {
    if (filterLanguage === '') {
      return recent.filter(word => word.language !== 'UNK');
    }
    return recent.filter(word => word.language === filterLanguage);
  }, [filterLanguage, recent]);

  return (
    <Paper elevation={3} sx={{ p: 2 }}>
      <Box
        display="flex"
        alignItems="center"
        justifyContent="space-between"
        flexWrap="wrap"
        gap={1}
      >
        <Typography variant="h6">Recently&nbsp;Learned&nbsp;Words</Typography>
        <FormControl variant="outlined" size="small" sx={{ minWidth: 120 }}>
          <InputLabel id="filter-language-label">Language</InputLabel>
          <Select
            labelId="filter-language-label"
            id="filter-language-select"
            value={filterLanguage}
            label="Language"
            onChange={(e) => setFilterLanguage(e.target.value)}
          >
            <MenuItem value="">
              <em>All</em>
            </MenuItem>
            {uniqueLanguages.map((lang) => (
              <MenuItem key={lang} value={lang}>
                {lang.toUpperCase()}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      {filteredWords.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          {recent.length === 0 ? "No words recorded yet." : "No words for selected language."}
        </Typography>
      ) : (
        <Box mt={1} display="flex" flexWrap="wrap" gap={1}>
          {filteredWords.map((w) => {
            const langColorProps = getLanguageColor(w.language);
            return (
              <Chip
                key={`${w.word}-${w.language}`}
                label={`${w.word} · ${w.language.toUpperCase()}`}
                sx={{ fontWeight: 500, ...langColorProps }}
              />
            );
          })}
        </Box>
      )}
    </Paper>
  );
};

export default LearnedWords;
