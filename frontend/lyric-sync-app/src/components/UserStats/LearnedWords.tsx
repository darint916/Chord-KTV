import React from 'react';
import {
  Box,
  Chip,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import { getLanguageColor } from '../../utils/languageColorUtils';
import styles from './LearnedWords.module.scss';
import { LearnedWordDto } from '../../api';

interface LearnedWordsProps {
  recent: LearnedWordDto[];
}

const LearnedWords: React.FC<LearnedWordsProps> = ({ recent }) => {
  // State for the selected language filter. An empty string represents "All"
  const [filterLanguage, setFilterLanguage] = React.useState<string>('');

  // Compute the unique languages from the recent words list, excluding "UNK"
  const uniqueLanguages = React.useMemo(() => {
    const languages = new Set(recent.map((word) => word.language).filter((lang) => lang !== 'UNK'));
    return Array.from(languages).sort();
  }, [recent]);

  // Filter words based on the selected language filter, excluding "UNK"
  const filteredWords = React.useMemo(() => {
    if (filterLanguage === '') {
      return recent.filter((word) => word.language !== 'UNK');
    }
    return recent.filter((word) => word.language === filterLanguage);
  }, [filterLanguage, recent]);

  return (
    <Paper
      elevation={3}
      className={styles.learnedWordsPaper}
    >
      <Box className={styles.headerRow}>
        <Typography variant="h6">Learned&nbsp;Words</Typography>
        
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
                {(lang ?? 'UNK').toUpperCase()}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
        <Box component="hr" className={styles.divider} />
      </Box>

      {filteredWords.length === 0 ? (
        <Typography variant="body2" className={styles.emptyState}>
          {recent.length === 0 ? 'No words recorded yet.' : 'No words for selected language.'}
        </Typography>
      ) : (
        <Box className={styles.scrollContainer}>
          <Box className={styles.chipRow}>
            {filteredWords.map((w) => {
              const lang = w.language ?? 'UNK';
              const langColorProps = getLanguageColor(lang);
              return (
                <Chip
                  key={`${w.word}-${w.language}`}
                  label={`${w.word} · ${lang.toUpperCase()}`}
                  sx={{ fontWeight: 500, ...langColorProps }}
                />
              );
            })}
          </Box>
        </Box>
      )}
    </Paper>
  );
};

export default LearnedWords;
