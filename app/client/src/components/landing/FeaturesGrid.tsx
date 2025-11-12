interface FeatureCardProps {
  title: string;
  description: string;
  icon: React.ReactNode;
  gradientFrom: string;
  gradientTo: string;
  borderColor: string;
  bgColor: string;
}

function FeatureCard({ title, description, icon, gradientFrom, gradientTo, borderColor, bgColor }: FeatureCardProps) {
  return (
    <div className={`flex items-start gap-4 p-4 rounded-xl bg-linear-to-r ${gradientFrom} ${gradientTo} border ${borderColor} transition-all hover:shadow-md`}>
      <div className={`shrink-0 w-10 h-10 ${bgColor} rounded-lg flex items-center justify-center shadow-sm`}>
        {icon}
      </div>
      <div>
        <h3 className="font-bold text-slate-900 text-lg mb-1">{title}</h3>
        <p className="text-slate-600">{description}</p>
      </div>
    </div>
  );
}

export function FeaturesGrid() {
  return (
    <div className="space-y-5 mb-10">
      <FeatureCard
        title="AI-Powered Playlist Creation"
        description="Create custom playlists with natural language"
        icon={
          <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        }
        gradientFrom="from-green-50"
        gradientTo="to-emerald-50"
        borderColor="border-green-100"
        bgColor="bg-green-500"
      />

      <FeatureCard
        title="Personalized Recommendations"
        description="Discover new music based on your taste"
        icon={
          <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        }
        gradientFrom="from-blue-50"
        gradientTo="to-cyan-50"
        borderColor="border-blue-100"
        bgColor="bg-blue-500"
      />

      <FeatureCard
        title="Smart Cleanup Tools"
        description="Remove duplicates and organize your library"
        icon={
          <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
          </svg>
        }
        gradientFrom="from-purple-50"
        gradientTo="to-violet-50"
        borderColor="border-purple-100"
        bgColor="bg-purple-500"
      />
    </div>
  );
}
