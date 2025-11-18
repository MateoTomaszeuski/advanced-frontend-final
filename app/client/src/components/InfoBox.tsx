import { useState } from 'react';

interface InfoBoxProps {
  type: 'tips' | 'info';
  title?: string;
  items: string[];
  defaultExpanded?: boolean;
}

export function InfoBox({ type, title, items, defaultExpanded = false }: InfoBoxProps) {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded);

  const config = {
    tips: {
      emoji: '💡',
      title: title || 'Tips',
      bgColor: 'bg-theme-card',
      borderColor: 'border-theme-border',
      textColor: 'text-theme-text',
      subtextColor: 'text-theme-text opacity-80',
      iconColor: 'text-theme-accent',
      opacityClass: 'opacity-100',
    },
    info: {
      emoji: 'ℹ️',
      title: title || 'How it works',
      bgColor: 'bg-theme-card',
      borderColor: 'border-theme-border',
      textColor: 'text-theme-text',
      subtextColor: 'text-theme-text opacity-80',
      iconColor: 'text-theme-primary',
      opacityClass: 'opacity-90',
    },
  };

  const style = config[type];

  return (
    <div className={`${style.bgColor} ${style.opacityClass} border ${style.borderColor} rounded-lg p-4`}>
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between gap-3 text-left focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-theme-accent rounded"
      >
        <div className="flex items-center gap-2">
          <span className="text-lg">{style.emoji}</span>
          <h3 className={`font-medium ${style.textColor}`}>{style.title}</h3>
        </div>
        <svg
          className={`w-5 h-5 ${style.iconColor} transform transition-transform duration-300 ${
            isExpanded ? 'rotate-180' : ''
          }`}
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>
      
      <div
        className={`transition-all duration-300 ease-in-out ${
          isExpanded ? 'opacity-100 mt-3' : 'opacity-0 max-h-0 overflow-hidden'
        }`}
      >
        {isExpanded && (
          <ul className={`text-sm ${style.subtextColor} space-y-1`}>
            {items.map((item, index) => (
              <li key={index}>• {item}</li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
