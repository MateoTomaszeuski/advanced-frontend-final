import type { ReactNode } from 'react';

interface MetricCardProps {
  label: string;
  value: string | number;
  icon: ReactNode;
  bgColor?: string;
  iconBgColor?: string;
  iconColor?: string;
  isLoading?: boolean;
}

export function MetricCard({
  label,
  value,
  icon,
  bgColor = 'bg-theme-card',
  iconBgColor = 'bg-gray-100',
  iconColor = 'text-gray-700',
  isLoading = false,
}: MetricCardProps) {
  return (
    <div className={`${bgColor} rounded-lg shadow p-6`}>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-theme-text opacity-70">{label}</p>
          {isLoading ? (
            <div className="h-8 w-16 bg-gray-200 animate-pulse rounded mt-1" />
          ) : (
            <p className="text-2xl font-bold text-theme-text">{value}</p>
          )}
        </div>
        <div className={`w-12 h-12 rounded-full ${iconBgColor} flex items-center justify-center`}>
          <div className={iconColor}>{icon}</div>
        </div>
      </div>
    </div>
  );
}
