import { forwardRef } from 'react';
import type { InputHTMLAttributes } from 'react';

export interface TextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
}

export const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
  ({ label, error, helperText, className = '', ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={props.id} className="text-sm font-medium text-theme-text">
            {label}
            {props.required && <span className="text-red-500 ml-1">*</span>}
          </label>
        )}
                  <input
            ref={ref}
            className={`
              px-3 py-2 border rounded-lg
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
              disabled:bg-gray-100 disabled:cursor-not-allowed
              ${error ? 'border-red-500' : 'border-theme-border'}
              ${className}
            `}
            {...props}
          />
        {error && <span className="text-sm text-red-600">{error}</span>}
        {helperText && !error && (
          <span className="text-sm text-theme-text">{helperText}</span>
        )}
      </div>
    );
  }
);

TextInput.displayName = 'TextInput';
