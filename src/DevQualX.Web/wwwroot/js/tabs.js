/**
 * Tabs Progressive Enhancement
 * 
 * Enhances static tabs with JavaScript functionality:
 * - Click handling to switch tabs without page reload
 * - Keyboard navigation (Arrow keys, Home, End)
 * - URL hash support for deep linking
 * - Works without JavaScript (degrades gracefully to anchor links)
 */

(function() {
    'use strict';

    /**
     * Initialize tabs functionality
     */
    function initTabs() {
        const tabContainers = document.querySelectorAll('[data-tabs]');
        
        tabContainers.forEach(container => {
            const tabs = container.querySelectorAll('[role="tab"]');
            const panels = container.querySelectorAll('[role="tabpanel"]');
            
            if (tabs.length === 0 || panels.length === 0) return;
            
            // Activate tab based on URL hash on page load
            const hash = window.location.hash;
            if (hash) {
                const targetPanel = container.querySelector(hash);
                if (targetPanel) {
                    const targetTab = container.querySelector(`[aria-controls="${hash.substring(1)}"]`);
                    if (targetTab) {
                        activateTab(targetTab, tabs, panels);
                    }
                }
            }
            
            // Click event handlers
            tabs.forEach(tab => {
                tab.addEventListener('click', (e) => {
                    e.preventDefault();
                    activateTab(tab, tabs, panels);
                    
                    // Update URL hash without scrolling
                    const panelId = tab.getAttribute('aria-controls');
                    if (panelId) {
                        history.replaceState(null, '', `#${panelId}`);
                    }
                });
            });
            
            // Keyboard navigation
            container.querySelector('[role="tablist"]')?.addEventListener('keydown', (e) => {
                const currentTab = document.activeElement;
                
                // Only handle keyboard if focus is on a tab
                if (!currentTab.matches('[role="tab"]')) return;
                
                const tabsArray = Array.from(tabs);
                const currentIndex = tabsArray.indexOf(currentTab);
                let targetTab = null;
                
                switch (e.key) {
                    case 'ArrowLeft':
                    case 'ArrowUp':
                        e.preventDefault();
                        targetTab = tabsArray[currentIndex - 1] || tabsArray[tabsArray.length - 1];
                        break;
                    case 'ArrowRight':
                    case 'ArrowDown':
                        e.preventDefault();
                        targetTab = tabsArray[currentIndex + 1] || tabsArray[0];
                        break;
                    case 'Home':
                        e.preventDefault();
                        targetTab = tabsArray[0];
                        break;
                    case 'End':
                        e.preventDefault();
                        targetTab = tabsArray[tabsArray.length - 1];
                        break;
                    default:
                        return;
                }
                
                if (targetTab) {
                    activateTab(targetTab, tabs, panels);
                    targetTab.focus();
                    
                    // Update URL hash
                    const panelId = targetTab.getAttribute('aria-controls');
                    if (panelId) {
                        history.replaceState(null, '', `#${panelId}`);
                    }
                }
            });
        });
    }
    
    /**
     * Activate a specific tab and its corresponding panel
     */
    function activateTab(selectedTab, allTabs, allPanels) {
        const selectedPanelId = selectedTab.getAttribute('aria-controls');
        
        // Deactivate all tabs
        allTabs.forEach(tab => {
            tab.setAttribute('aria-selected', 'false');
            tab.setAttribute('tabindex', '-1');
            tab.classList.remove('tabs__button--active');
        });
        
        // Hide all panels
        allPanels.forEach(panel => {
            panel.setAttribute('hidden', '');
            panel.classList.remove('tabs__panel--active');
        });
        
        // Activate selected tab
        selectedTab.setAttribute('aria-selected', 'true');
        selectedTab.setAttribute('tabindex', '0');
        selectedTab.classList.add('tabs__button--active');
        
        // Show selected panel
        const selectedPanel = document.getElementById(selectedPanelId);
        if (selectedPanel) {
            selectedPanel.removeAttribute('hidden');
            selectedPanel.classList.add('tabs__panel--active');
        }
    }
    
    /**
     * Handle hash changes (browser back/forward)
     */
    function handleHashChange() {
        const hash = window.location.hash;
        if (!hash) return;
        
        const targetPanel = document.querySelector(hash);
        if (!targetPanel || targetPanel.getAttribute('role') !== 'tabpanel') return;
        
        const container = targetPanel.closest('[data-tabs]');
        if (!container) return;
        
        const tabs = container.querySelectorAll('[role="tab"]');
        const panels = container.querySelectorAll('[role="tabpanel"]');
        const targetTab = container.querySelector(`[aria-controls="${hash.substring(1)}"]`);
        
        if (targetTab) {
            activateTab(targetTab, tabs, panels);
        }
    }
    
    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTabs);
    } else {
        initTabs();
    }
    
    // Handle hash changes (back/forward navigation)
    window.addEventListener('hashchange', handleHashChange);
    
    // Reinitialize after Blazor enhanced navigation
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancednavigation', initTabs);
    }
    
    // Expose API for manual initialization
    window.DevQualX = window.DevQualX || {};
    window.DevQualX.tabs = {
        init: initTabs
    };
})();
