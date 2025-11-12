import type { ReactNode } from 'react';

interface QuickActionCardProps {
  title: string;
  description: string;
  icon: ReactNode;
  color: string;
  onClick: () => void;
}

export function QuickActionCard({ title, description, icon, color, onClick }: QuickActionCardProps) {
  return (
    <button
      onClick={onClick}
      className="flex flex-col items-start p-4 border-2 border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group"
    >
      <div className={`${color} p-3 rounded-lg mb-3 group-hover:scale-110 transition-transform`}>
        {icon}
      </div>
      <h3 className="text-sm font-semibold text-gray-900 mb-1 group-hover:text-green-700 transition-colors">
        {title}
      </h3>
      <p className="text-xs text-gray-500">{description}</p>
    </button>
  );
}
