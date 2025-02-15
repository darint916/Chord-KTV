import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import * as reactPlugin from 'eslint-plugin-react';

export default tseslint.config(
  { ignores: ['dist'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended, ...reactPlugin.configs.recommended],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
    },
    rules: {
      'no-console': 'error', // Disallow console logs
      'no-unused-vars': ['error', { argsIgnorePattern: '^_' }], // No unused variables, except those prefixed with "_"
      'eqeqeq': 'error', // Enforce strict equality (=== and !==)
      'curly': 'error', // Require curly braces for all control statements
      'semi': ['error', 'always'], // Require semicolons
      'quotes': ['error', 'single'], // Enforce single quotes
      'indent': ['error', 2], // Enforce 2-space indentation
      'no-var': 'error', // Disallow `var`, use `let` or `const`
      'prefer-const': 'error', // Enforce `const` when variables are not reassigned
      'no-debugger': 'error', // Disallow `debugger` statements
      '@typescript-eslint/no-explicit-any': 'error', // Disallow `any` type in TypeScript
      '@typescript-eslint/no-non-null-assertion': 'error', // Disallow `!` non-null assertions
      'react-refresh/only-export-components': [
        'warn',
        { allowConstantExport: true },
      ],
    },
  },
)
