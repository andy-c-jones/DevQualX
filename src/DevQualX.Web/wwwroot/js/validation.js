/**
 * Form Validation with Progressive Enhancement
 * Enhances HTML5 validation with custom styled error messages
 */

(function() {
  'use strict';

  // Configuration
  const config = {
    validateOnBlur: true,
    validateOnInput: false,
    errorClass: 'form-field__input--error',
    errorMessageClass: 'form-field__error'
  };

  /**
   * Get custom validation message from data attribute or use browser default
   */
  function getValidationMessage(field) {
    const customMessage = field.dataset.validationMessage;
    if (customMessage) {
      return customMessage;
    }

    const validity = field.validity;
    
    if (validity.valueMissing) {
      return field.dataset.validationRequiredMessage || 'This field is required.';
    }
    if (validity.typeMismatch) {
      if (field.type === 'email') {
        return field.dataset.validationEmailMessage || 'Please enter a valid email address.';
      }
      if (field.type === 'url') {
        return field.dataset.validationUrlMessage || 'Please enter a valid URL.';
      }
    }
    if (validity.patternMismatch) {
      return field.dataset.validationPatternMessage || 'Please match the requested format.';
    }
    if (validity.tooShort) {
      return field.dataset.validationMinlengthMessage || `Please enter at least ${field.minLength} characters.`;
    }
    if (validity.tooLong) {
      return field.dataset.validationMaxlengthMessage || `Please enter no more than ${field.maxLength} characters.`;
    }
    if (validity.rangeUnderflow) {
      return field.dataset.validationMinMessage || `Value must be ${field.min} or higher.`;
    }
    if (validity.rangeOverflow) {
      return field.dataset.validationMaxMessage || `Value must be ${field.max} or lower.`;
    }
    
    return field.validationMessage;
  }

  /**
   * Display error message for a field
   */
  function showFieldError(field) {
    const errorContainer = getErrorContainer(field);
    if (!errorContainer) return;

    const message = getValidationMessage(field);
    errorContainer.textContent = message;
    errorContainer.classList.add(config.errorMessageClass);
    field.classList.add(config.errorClass);
    field.setAttribute('aria-invalid', 'true');
  }

  /**
   * Clear error message for a field
   */
  function clearFieldError(field) {
    const errorContainer = getErrorContainer(field);
    if (!errorContainer) return;

    errorContainer.textContent = '';
    errorContainer.classList.remove(config.errorMessageClass);
    field.classList.remove(config.errorClass);
    field.setAttribute('aria-invalid', 'false');
  }

  /**
   * Get or create error container for a field
   */
  function getErrorContainer(field) {
    const fieldWrapper = field.closest('.form-field');
    if (!fieldWrapper) return null;

    let descriptionEl = fieldWrapper.querySelector('.form-field__description');
    if (!descriptionEl) {
      descriptionEl = document.createElement('div');
      descriptionEl.className = 'form-field__description';
      descriptionEl.id = `${field.id}-description`;
      fieldWrapper.appendChild(descriptionEl);
      field.setAttribute('aria-describedby', descriptionEl.id);
    }

    let errorEl = descriptionEl.querySelector('.form-field__error');
    if (!errorEl) {
      errorEl = document.createElement('span');
      errorEl.className = 'form-field__error';
      descriptionEl.appendChild(errorEl);
    }

    return errorEl;
  }

  /**
   * Validate a single field
   */
  function validateField(field) {
    if (!field.validity.valid) {
      showFieldError(field);
      return false;
    } else {
      clearFieldError(field);
      return true;
    }
  }

  /**
   * Validate all fields in a form
   */
  function validateForm(form) {
    const fields = form.querySelectorAll('input:not([type="hidden"]), textarea, select');
    let isValid = true;

    fields.forEach(field => {
      if (!validateField(field)) {
        isValid = false;
      }
    });

    return isValid;
  }

  /**
   * Initialize validation for a form
   */
  function initializeForm(form) {
    // Disable browser's default validation tooltips
    form.setAttribute('novalidate', '');

    // Handle form submission
    form.addEventListener('submit', function(e) {
      if (!validateForm(form)) {
        e.preventDefault();
        e.stopPropagation();

        // Focus first invalid field
        const firstInvalid = form.querySelector(`.${config.errorClass}`);
        if (firstInvalid) {
          firstInvalid.focus();
        }
      }
    });

    // Handle field validation on blur
    if (config.validateOnBlur) {
      const fields = form.querySelectorAll('input:not([type="hidden"]), textarea, select');
      fields.forEach(field => {
        field.addEventListener('blur', function() {
          if (field.value || field.hasAttribute('required')) {
            validateField(field);
          }
        });
      });
    }

    // Handle field validation on input (clear errors)
    if (config.validateOnInput) {
      const fields = form.querySelectorAll('input:not([type="hidden"]), textarea, select');
      fields.forEach(field => {
        field.addEventListener('input', function() {
          if (field.classList.contains(config.errorClass)) {
            validateField(field);
          }
        });
      });
    } else {
      // At minimum, clear error styling when user starts typing
      const fields = form.querySelectorAll('input:not([type="hidden"]), textarea, select');
      fields.forEach(field => {
        field.addEventListener('input', function() {
          if (field.classList.contains(config.errorClass)) {
            clearFieldError(field);
          }
        });
      });
    }
  }

  /**
   * Initialize all forms on the page
   */
  function init() {
    const forms = document.querySelectorAll('form');
    forms.forEach(initializeForm);
  }

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

  // Re-initialize when Blazor finishes rendering (for SSR -> interactive)
  if (window.Blazor) {
    window.Blazor.addEventListener('enhancedload', init);
  }

  // Expose API for manual validation
  window.DevQualX = window.DevQualX || {};
  window.DevQualX.validation = {
    validateField,
    validateForm,
    showFieldError,
    clearFieldError
  };
})();
