import { forwardRef } from 'react';
import type { InputHTMLAttributes } from 'react';

export interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label?: string;
  error?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ label, error, className = '', ...props }, ref) => {
    return (
      <div className="flex flex-col gap-1">
        <div className="flex items-center gap-2">
          <input
            ref={ref}
            type="checkbox"
            className={`
              h-4 w-4 rounded border-theme-border
              text-primary-600 focus:ring-2 focus:ring-primary-500
              disabled:cursor-not-allowed disabled:opacity-50
              ${error ? 'border-red-500' : ''}
              ${className}
            `}
            {...props}
          />
          {label && (
            <label htmlFor={props.id} className="text-sm text-theme-text cursor-pointer">
              {label}
            </label>
          )}
        </div>
        {error && <span className="text-sm text-red-600 ml-6">{error}</span>}
      </div>
    );
  }
);

Checkbox.displayName = 'Checkbox';
