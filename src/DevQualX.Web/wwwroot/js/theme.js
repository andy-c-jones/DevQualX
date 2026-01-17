/**
 * Theme Switcher
 * 
 * Manages light/dark/system theme preferences:
 * - Reads theme from localStorage
 * - Applies theme to document
 * - Listens for system preference changes
 * - Provides API for theme switching
 */

(function() {
    'use strict';
    
    const STORAGE_KEY = 'devqualx-theme';
    const THEMES = {
        LIGHT: 'light',
        DARK: 'dark',
        SYSTEM: 'system'
    };
    
    /**
     * Get the current theme from localStorage or default to system
     */
    function getStoredTheme() {
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored && Object.values(THEMES).includes(stored) ? stored : THEMES.SYSTEM;
    }
    
    /**
     * Get the system theme preference
     */
    function getSystemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? THEMES.DARK : THEMES.LIGHT;
    }
    
    /**
     * Get the effective theme (resolves 'system' to actual theme)
     */
    function getEffectiveTheme(theme) {
        return theme === THEMES.SYSTEM ? getSystemTheme() : theme;
    }
    
    /**
     * Apply theme to document
     */
    function applyTheme(theme) {
        const effectiveTheme = getEffectiveTheme(theme);
        
        // Update document class
        document.documentElement.classList.remove(THEMES.LIGHT, THEMES.DARK);
        document.documentElement.classList.add(effectiveTheme);
        
        // Update data attribute for CSS targeting
        document.documentElement.setAttribute('data-theme', effectiveTheme);
        
        // Update meta theme-color for mobile browsers
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', effectiveTheme === THEMES.DARK ? '#1f2937' : '#ffffff');
        }
    }
    
    /**
     * Set theme preference
     */
    function setTheme(theme) {
        if (!Object.values(THEMES).includes(theme)) {
            console.error(`Invalid theme: ${theme}`);
            return;
        }
        
        localStorage.setItem(STORAGE_KEY, theme);
        applyTheme(theme);
        
        // Dispatch custom event for UI updates
        window.dispatchEvent(new CustomEvent('themechange', { 
            detail: { 
                theme, 
                effectiveTheme: getEffectiveTheme(theme) 
            } 
        }));
    }
    
    /**
     * Initialize theme on page load
     */
    function initTheme() {
        const theme = getStoredTheme();
        applyTheme(theme);
        
        // Listen for system theme changes
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
            const currentTheme = getStoredTheme();
            if (currentTheme === THEMES.SYSTEM) {
                applyTheme(THEMES.SYSTEM);
                window.dispatchEvent(new CustomEvent('themechange', { 
                    detail: { 
                        theme: THEMES.SYSTEM, 
                        effectiveTheme: e.matches ? THEMES.DARK : THEMES.LIGHT 
                    } 
                }));
            }
        });
    }
    
    // Initialize immediately (before DOM ready) to prevent flash
    initTheme();
    
    // Reinitialize after Blazor enhanced navigation
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancednavigation', initTheme);
    }
    
    // Expose API
    window.DevQualX = window.DevQualX || {};
    window.DevQualX.theme = {
        get: getStoredTheme,
        set: setTheme,
        getEffective: () => getEffectiveTheme(getStoredTheme()),
        THEMES
    };
})();
