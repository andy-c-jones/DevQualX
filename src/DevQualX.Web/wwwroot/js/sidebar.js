(function() {
    'use strict';
    
    let sidebar = null;
    let toggleButton = null;
    let overlay = null;
    let content = null;
    let isOpen = false;
    
    function initSidebar() {
        // Find sidebar elements
        sidebar = document.querySelector('[data-sidebar]');
        if (!sidebar) return;
        
        toggleButton = sidebar.querySelector('[data-sidebar-toggle]');
        overlay = sidebar.querySelector('[data-sidebar-overlay]');
        content = sidebar.querySelector('.sidebar__content');
        
        if (!toggleButton || !overlay || !content) return;
        
        // Set up event listeners
        toggleButton.addEventListener('click', handleToggle);
        overlay.addEventListener('click', closeSidebar);
        
        // Close on Escape key
        document.addEventListener('keydown', handleKeydown);
        
        // Close on navigation link click (only on mobile)
        const navLinks = sidebar.querySelectorAll('.sidebar__nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', handleNavLinkClick);
        });
    }
    
    function handleToggle(e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (isOpen) {
            closeSidebar();
        } else {
            openSidebar();
        }
    }
    
    function openSidebar() {
        isOpen = true;
        content.classList.add('sidebar__content--mobile-open');
        overlay.classList.add('sidebar__overlay--visible');
        toggleButton.setAttribute('aria-expanded', 'true');
        toggleButton.setAttribute('aria-label', 'Close menu');
        
        // Update icon to X mark
        const icon = toggleButton.querySelector('svg');
        if (icon) {
            icon.innerHTML = '<path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />';
        }
        
        // Prevent body scroll on mobile
        document.body.style.overflow = 'hidden';
    }
    
    function closeSidebar() {
        isOpen = false;
        content.classList.remove('sidebar__content--mobile-open');
        overlay.classList.remove('sidebar__overlay--visible');
        toggleButton.setAttribute('aria-expanded', 'false');
        toggleButton.setAttribute('aria-label', 'Open menu');
        
        // Update icon to hamburger
        const icon = toggleButton.querySelector('svg');
        if (icon) {
            icon.innerHTML = '<path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />';
        }
        
        // Restore body scroll
        document.body.style.overflow = '';
    }
    
    function handleKeydown(e) {
        if (e.key === 'Escape' && isOpen) {
            closeSidebar();
        }
    }
    
    function handleNavLinkClick() {
        // Close sidebar on mobile when clicking a nav link
        if (window.innerWidth < 641 && isOpen) {
            closeSidebar();
        }
    }
    
    function cleanup() {
        if (toggleButton) {
            toggleButton.removeEventListener('click', handleToggle);
        }
        if (overlay) {
            overlay.removeEventListener('click', closeSidebar);
        }
        document.removeEventListener('keydown', handleKeydown);
        
        // Restore body scroll if sidebar was open
        if (isOpen) {
            document.body.style.overflow = '';
        }
        
        // Reset state
        sidebar = null;
        toggleButton = null;
        overlay = null;
        content = null;
        isOpen = false;
    }
    
    // Initialize immediately
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSidebar);
    } else {
        initSidebar();
    }
    
    // Reinitialize after Blazor enhanced navigation
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancednavigation', function() {
            cleanup();
            // Wait a tick for DOM to update
            setTimeout(initSidebar, 0);
        });
    }
    
    // Expose API
    window.DevQualX = window.DevQualX || {};
    window.DevQualX.sidebar = {
        init: initSidebar,
        open: openSidebar,
        close: closeSidebar,
        toggle: handleToggle,
        cleanup: cleanup
    };
})();
