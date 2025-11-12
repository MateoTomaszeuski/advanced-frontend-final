import { useAuth } from 'react-oidc-context';
import { AuthLayout } from '../components/layout/AuthLayout';
import { HeroSection } from '../components/landing/HeroSection';
import { FeaturesGrid } from '../components/landing/FeaturesGrid';
import { CTASection } from '../components/landing/CTASection';

export function LandingPage() {
  const auth = useAuth();

  if (auth.isAuthenticated) {
    window.location.href = '/dashboard';
    return null;
  }

  return (
    <AuthLayout>
      <div className="bg-white rounded-2xl shadow-2xl p-10 border border-slate-100">
        <HeroSection />
        <FeaturesGrid />
        <CTASection onSignIn={() => auth.signinRedirect()} />
      </div>
    </AuthLayout>
  );
}

