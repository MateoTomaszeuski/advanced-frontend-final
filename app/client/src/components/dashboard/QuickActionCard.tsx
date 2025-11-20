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
      className="flex flex-col items-start p-4 border-2 border-theme-border rounded-lg hover:text-theme-border group-hover:border-theme-accent hover:shadow-md transition-all group"
    >
      <div className={`${color} p-3 rounded-lg mb-3 group-hover:scale-110 transition-transform [&_svg]:stroke-current`}>
        {icon}
      </div>
      <h3 className="text-sm font-semibold text-theme-text mb-1 group-hover:text-theme-accent transition-colors">
        {title}
      </h3>
      <p className="text-xs text-theme-text opacity-70">{description}</p>
    </button>
  );
}
