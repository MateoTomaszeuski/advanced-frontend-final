import { forwardRef } from 'react';
import type { SelectHTMLAttributes } from 'react';

export interface SelectOption {
  value: string;
  label: string;
  disabled?: boolean;
}

export interface SelectDropdownProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  helperText?: string;
  options: SelectOption[];
  placeholder?: string;
}

export const SelectDropdown = forwardRef<HTMLSelectElement, SelectDropdownProps>(
  ({ label, error, helperText, options, placeholder, className = '', ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1.5">
        {label && (
          <label htmlFor={props.id} className="text-sm font-medium text-theme-text">
            {label}
            {props.required && <span className="text-red-500 ml-1">*</span>}
          </label>
        )}
        <select
          ref={ref}
          className={`
            px-3 py-2 border rounded-lg
            bg-theme-card text-theme-text
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            disabled:opacity-60 disabled:cursor-not-allowed
            ${error ? 'border-red-500' : 'border-theme-border'}
            ${className}
          `}
          {...props}
        >
          {placeholder && (
            <option value="" disabled className="bg-theme-card text-theme-text">
              {placeholder}
            </option>
          )}
          {options.map((option) => (
            <option key={option.value} value={option.value} disabled={option.disabled} className="bg-theme-card text-theme-text">
              {option.label}
            </option>
          ))}
        </select>
        {error && <span className="text-sm text-red-600">{error}</span>}
        {helperText && !error && (
          <span className="text-sm text-theme-text">{helperText}</span>
        )}
      </div>
    );
  }
);

SelectDropdown.displayName = 'SelectDropdown';
