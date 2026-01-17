/**
 * Notification System
 * 
 * Client-side notification manager with:
 * - Toast notifications (temporary, top-right)
 * - Notification tray (persistent history)
 * - localStorage persistence
 * - Progress notifications for async operations
 * - Auto-cleanup (7 days old, max 100 items)
 */

(function() {
    'use strict';
    
    const STORAGE_KEY = 'devqualx-notifications';
    const MAX_NOTIFICATIONS = 100;
    const MAX_AGE_MS = 7 * 24 * 60 * 60 * 1000; // 7 days
    const MAX_ACTIVE_TOASTS = 5;
    const DEFAULT_TIMEOUT_MS = 10000; // 10 seconds
    
    const LEVELS = {
        PRIMARY: 'primary',
        SECONDARY: 'secondary',
        SUCCESS: 'success',
        DANGER: 'danger',
        WARNING: 'warning',
        INFO: 'info'
    };
    
    const TYPES = {
        STANDARD: 'standard',
        PROGRESS: 'progress'
    };
    
    let notifications = [];
    let activeToasts = [];
    let progressIntervals = new Map(); // Track progress timers
    
    /**
     * Generate a unique ID for notifications
     */
    function generateId() {
        return `notif-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }
    
    /**
     * Load notifications from localStorage
     */
    function loadFromStorage() {
        try {
            const stored = localStorage.getItem(STORAGE_KEY);
            if (stored) {
                notifications = JSON.parse(stored);
                cleanup();
            }
        } catch (error) {
            console.error('Failed to load notifications from localStorage:', error);
            notifications = [];
        }
    }
    
    /**
     * Save notifications to localStorage
     */
    function saveToStorage() {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(notifications));
        } catch (error) {
            console.error('Failed to save notifications to localStorage:', error);
        }
    }
    
    /**
     * Clean up old notifications
     * - Remove notifications older than 7 days
     * - Keep only last 100 notifications
     */
    function cleanup() {
        const now = Date.now();
        
        // Remove old notifications
        notifications = notifications.filter(n => (now - n.timestamp) < MAX_AGE_MS);
        
        // Keep only last 100 notifications (sorted by timestamp desc)
        if (notifications.length > MAX_NOTIFICATIONS) {
            notifications.sort((a, b) => b.timestamp - a.timestamp);
            notifications = notifications.slice(0, MAX_NOTIFICATIONS);
        }
        
        saveToStorage();
    }
    
    /**
     * Dispatch custom event for Blazor components
     */
    function dispatchEvent(eventName, detail) {
        window.dispatchEvent(new CustomEvent(eventName, { detail }));
    }
    
    /**
     * Format relative time (just now, 5m ago, 1h ago, etc.)
     */
    function formatRelativeTime(timestamp) {
        const now = Date.now();
        const diff = now - timestamp;
        
        const seconds = Math.floor(diff / 1000);
        if (seconds < 10) return 'just now';
        if (seconds < 60) return `${seconds}s ago`;
        
        const minutes = Math.floor(seconds / 60);
        if (minutes < 60) return `${minutes}m ago`;
        
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        
        const days = Math.floor(hours / 24);
        if (days < 7) return `${days}d ago`;
        
        return new Date(timestamp).toLocaleDateString();
    }
    
    /**
     * Format elapsed time (00:05, 01:23, etc.)
     */
    function formatElapsedTime(ms) {
        const totalSeconds = Math.floor(ms / 1000);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;
        return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
    
    /**
     * Add notification to active toasts queue
     */
    function addToActiveToasts(id) {
        if (!activeToasts.includes(id)) {
            activeToasts.push(id);
            
            // If we exceed max toasts, queue will be processed as toasts dismiss
            if (activeToasts.length > MAX_ACTIVE_TOASTS) {
                // Keep only the last MAX_ACTIVE_TOASTS
                activeToasts = activeToasts.slice(-MAX_ACTIVE_TOASTS);
            }
        }
    }
    
    /**
     * Remove notification from active toasts
     */
    function removeFromActiveToasts(id) {
        const index = activeToasts.indexOf(id);
        if (index > -1) {
            activeToasts.splice(index, 1);
        }
    }
    
    /**
     * Show a standard notification
     */
    function show(options) {
        const notification = {
            id: generateId(),
            title: options.title || 'Notification',
            message: options.message || '',
            level: options.level || LEVELS.INFO,
            type: TYPES.STANDARD,
            timestamp: Date.now(),
            read: false,
            dismissed: false,
            timeout: options.timeout !== undefined ? options.timeout : DEFAULT_TIMEOUT_MS,
            dismissible: options.dismissible !== undefined ? options.dismissible : true
        };
        
        notifications.unshift(notification); // Add to beginning (newest first)
        saveToStorage();
        
        addToActiveToasts(notification.id);
        dispatchEvent('devqualx-notification-added', notification);
        
        // Auto-dismiss after timeout
        if (notification.timeout > 0) {
            setTimeout(() => {
                dismiss(notification.id);
            }, notification.timeout);
        }
        
        return notification.id;
    }
    
    /**
     * Show a progress notification
     */
    function showProgress(options) {
        const notification = {
            id: generateId(),
            title: options.title || 'Processing...',
            message: options.message || 'Please wait...',
            level: LEVELS.PRIMARY,
            type: TYPES.PROGRESS,
            timestamp: Date.now(),
            read: false,
            dismissed: false,
            timeout: 0, // Progress notifications don't auto-dismiss
            dismissible: false, // Can't manually dismiss while in progress
            elapsedMs: 0,
            startTime: Date.now()
        };
        
        notifications.unshift(notification);
        saveToStorage();
        
        addToActiveToasts(notification.id);
        dispatchEvent('devqualx-notification-added', notification);
        
        // Start elapsed time counter
        const intervalId = setInterval(() => {
            updateProgress(notification.id, Date.now() - notification.startTime);
        }, 1000);
        
        progressIntervals.set(notification.id, intervalId);
        
        return notification.id;
    }
    
    /**
     * Update progress notification elapsed time
     */
    function updateProgress(id, elapsedMs) {
        const notification = notifications.find(n => n.id === id);
        if (notification && notification.type === TYPES.PROGRESS) {
            notification.elapsedMs = elapsedMs;
            saveToStorage();
            dispatchEvent('devqualx-notification-progress-updated', { id, elapsedMs });
        }
    }
    
    /**
     * Complete a progress notification
     */
    function completeProgress(id, success, message, href) {
        const notification = notifications.find(n => n.id === id);
        if (!notification || notification.type !== TYPES.PROGRESS) {
            return;
        }
        
        // Stop the elapsed time counter
        const intervalId = progressIntervals.get(id);
        if (intervalId) {
            clearInterval(intervalId);
            progressIntervals.delete(id);
        }
        
        // Transform to standard notification
        notification.type = TYPES.STANDARD;
        notification.level = success ? LEVELS.SUCCESS : LEVELS.DANGER;
        notification.title = success ? (notification.title.replace('Processing', 'Complete')) : 'Failed';
        notification.message = message || notification.message;
        notification.dismissible = true;
        notification.timeout = 5000; // Auto-dismiss after 5 seconds
        notification.href = href || null;
        
        saveToStorage();
        dispatchEvent('devqualx-notification-progress-completed', notification);
        
        // Auto-dismiss after timeout
        setTimeout(() => {
            dismiss(id);
        }, notification.timeout);
    }
    
    /**
     * Dismiss a notification (remove from active toasts, but keep in tray)
     */
    function dismiss(id) {
        const notification = notifications.find(n => n.id === id);
        if (notification) {
            notification.dismissed = true;
            saveToStorage();
            
            removeFromActiveToasts(id);
            dispatchEvent('devqualx-notification-dismissed', { id });
        }
    }
    
    /**
     * Remove a notification completely (from tray too)
     */
    function remove(id) {
        const index = notifications.findIndex(n => n.id === id);
        if (index > -1) {
            const notification = notifications[index];
            
            // Stop progress timer if exists
            const intervalId = progressIntervals.get(id);
            if (intervalId) {
                clearInterval(intervalId);
                progressIntervals.delete(id);
            }
            
            notifications.splice(index, 1);
            saveToStorage();
            
            removeFromActiveToasts(id);
            dispatchEvent('devqualx-notification-removed', { id });
        }
    }
    
    /**
     * Clear all notifications
     */
    function clearAll() {
        // Stop all progress timers
        progressIntervals.forEach((intervalId) => {
            clearInterval(intervalId);
        });
        progressIntervals.clear();
        
        notifications = [];
        activeToasts = [];
        saveToStorage();
        
        dispatchEvent('devqualx-notifications-cleared', {});
    }
    
    /**
     * Mark notification as read
     */
    function markAsRead(id) {
        const notification = notifications.find(n => n.id === id);
        if (notification) {
            notification.read = true;
            saveToStorage();
            dispatchEvent('devqualx-notification-read', { id });
        }
    }
    
    /**
     * Get all notifications (sorted by timestamp desc)
     */
    function getAll() {
        return notifications.slice().sort((a, b) => b.timestamp - a.timestamp);
    }
    
    /**
     * Get active (non-dismissed) notifications for toast rendering
     */
    function getActive() {
        return notifications
            .filter(n => !n.dismissed)
            .sort((a, b) => b.timestamp - a.timestamp)
            .slice(0, MAX_ACTIVE_TOASTS);
    }
    
    /**
     * Get unread notification count
     */
    function getUnreadCount() {
        return notifications.filter(n => !n.read).length;
    }
    
    /**
     * Get notification by ID
     */
    function getById(id) {
        return notifications.find(n => n.id === id);
    }
    
    /**
     * Initialize the notification system
     */
    function init() {
        loadFromStorage();
        
        // Re-render active toasts on page load
        const active = getActive();
        active.forEach(notification => {
            addToActiveToasts(notification.id);
            dispatchEvent('devqualx-notification-added', notification);
            
            // Restart progress timers
            if (notification.type === TYPES.PROGRESS) {
                const intervalId = setInterval(() => {
                    updateProgress(notification.id, Date.now() - notification.startTime);
                }, 1000);
                progressIntervals.set(notification.id, intervalId);
            }
        });
    }
    
    // Initialize on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    // Reinitialize after Blazor enhanced navigation
    if (window.Blazor) {
        window.Blazor.addEventListener('enhancednavigation', init);
    }
    
    // Expose public API
    window.DevQualX = window.DevQualX || {};
    window.DevQualX.notifications = {
        show,
        showProgress,
        updateProgress,
        completeProgress,
        dismiss,
        remove,
        clearAll,
        markAsRead,
        getAll,
        getActive,
        getUnreadCount,
        getById,
        formatRelativeTime,
        formatElapsedTime,
        init,
        LEVELS,
        TYPES
    };
})();
