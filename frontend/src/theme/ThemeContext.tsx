import React, { createContext, useContext, useState, useEffect, useMemo } from "react";
import { PaletteMode } from "@mui/material";
import { ThemeProvider as MuiThemeProvider } from "@mui/material/styles";
import { getTheme } from "../theme";

interface ThemeModeContextValue {
  mode: PaletteMode;
  toggleColorMode: () => void;
}

const ThemeModeContext = createContext<ThemeModeContextValue>({
  mode: "dark",
  toggleColorMode: () => {}
});

export function useThemeMode() {
  return useContext(ThemeModeContext);
}

export function AppThemeProvider({ children }: { children: React.ReactNode }) {
  const [mode, setMode] = useState<PaletteMode>(() => {
    const saved = localStorage.getItem("theme_mode");
    return (saved as PaletteMode) || "dark";
  });

  useEffect(() => {
    localStorage.setItem("theme_mode", mode);
    if (mode === "dark") {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }
  }, [mode]);

  const toggleColorMode = () => {
    setMode((prev) => (prev === "light" ? "dark" : "light"));
  };

  const theme = useMemo(() => getTheme(mode), [mode]);

  return (
    <ThemeModeContext.Provider value={{ mode, toggleColorMode }}>
      <MuiThemeProvider theme={theme}>
        {children}
      </MuiThemeProvider>
    </ThemeModeContext.Provider>
  );
}
