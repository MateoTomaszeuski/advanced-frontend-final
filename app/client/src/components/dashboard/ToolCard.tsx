import type { ReactNode } from 'react';

interface ToolCardProps {
  title: string;
  description: string;
  icon: ReactNode;
  iconBgColor: string;
  iconColor: string;
  onClick: () => void;
}

export function ToolCard({ title, description, icon, iconBgColor, iconColor, onClick }: ToolCardProps) {
  return (
    <button
      onClick={onClick}
      className="w-full flex items-center gap-4 p-4 border border-gray-200 rounded-lg hover:border-green-500 hover:shadow-md transition-all group text-left"
    >
      <div className={`${iconBgColor} p-3 rounded-lg group-hover:scale-110 transition-transform`}>
        <div className={iconColor}>{icon}</div>
      </div>
      <div className="flex-1">
        <h3 className="font-semibold text-gray-900 group-hover:text-green-700 transition-colors">
          {title}
        </h3>
        <p className="text-sm text-gray-500">{description}</p>
      </div>
    </button>
  );
}
