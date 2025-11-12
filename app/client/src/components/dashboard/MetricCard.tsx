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
  bgColor = 'bg-white',
  iconBgColor = 'bg-gray-100',
  iconColor = 'text-gray-700',
  isLoading = false,
}: MetricCardProps) {
  return (
    <div className={`${bgColor} rounded-lg shadow p-6`}>
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-600">{label}</p>
          {isLoading ? (
            <div className="h-8 w-16 bg-gray-200 animate-pulse rounded mt-1" />
          ) : (
            <p className="text-2xl font-bold text-gray-900">{value}</p>
          )}
        </div>
        <div className={`w-12 h-12 rounded-full ${iconBgColor} flex items-center justify-center`}>
          <div className={iconColor}>{icon}</div>
        </div>
      </div>
    </div>
  );
}
