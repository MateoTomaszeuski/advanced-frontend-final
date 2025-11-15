import { useState, useEffect } from 'react';
import { MainLayout } from '../components/layout/MainLayout';
import { Button } from '../components/forms/Button';
import { InfoBox } from '../components/InfoBox';
import { useTheme } from '../contexts/ThemeContext';
import { themeApi } from '../services/api';
import { showToast } from '../utils/toast';
import type { ThemeData } from '../types/theme';

const PREVIEW_THEME_KEY = 'preview-theme';
const PREVIEW_DESCRIPTION_KEY = 'preview-description';

export function CustomizePage() {
  const { applyTheme, resetTheme, hasCustomTheme, reloadTheme } = useTheme();
  const [description, setDescription] = useState('');
  const [isGenerating, setIsGenerating] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [generatedTheme, setGeneratedTheme] = useState<ThemeData | null>(null);

  // Restore preview theme from sessionStorage on mount
  useEffect(() => {
    const savedTheme = sessionStorage.getItem(PREVIEW_THEME_KEY);
    const savedDescription = sessionStorage.getItem(PREVIEW_DESCRIPTION_KEY);
    
    if (savedTheme) {
      try {
        const theme = JSON.parse(savedTheme) as ThemeData;
        setGeneratedTheme(theme);
        applyTheme(theme);
        if (savedDescription) {
          setDescription(savedDescription);
        }
      } catch (error) {
        console.error('Failed to restore preview theme:', error);
        sessionStorage.removeItem(PREVIEW_THEME_KEY);
        sessionStorage.removeItem(PREVIEW_DESCRIPTION_KEY);
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Only run on mount

  const handleGenerate = async () => {
    if (description.length < 10) {
      showToast.error('Please provide a more detailed description (at least 10 characters)');
      return;
    }

    setIsGenerating(true);
    try {
      const theme = await themeApi.generate({ description });
      setGeneratedTheme(theme);
      applyTheme(theme);
      // Save to sessionStorage so it persists when navigating
      sessionStorage.setItem(PREVIEW_THEME_KEY, JSON.stringify(theme));
      sessionStorage.setItem(PREVIEW_DESCRIPTION_KEY, description);
      showToast.success('Theme generated! Preview it below');
    } catch (error) {
      showToast.error('Failed to generate theme. Please try again.');
      console.error('Theme generation error:', error);
    } finally {
      setIsGenerating(false);
    }
  };

  const handleSave = async () => {
    if (!generatedTheme) return;

    setIsSaving(true);
    try {
      await themeApi.save({ themeData: generatedTheme, description });
      // Clear preview from sessionStorage
      sessionStorage.removeItem(PREVIEW_THEME_KEY);
      sessionStorage.removeItem(PREVIEW_DESCRIPTION_KEY);
      setGeneratedTheme(null);
      setDescription('');
      // Reload theme to update hasCustomTheme state
      await reloadTheme();
      showToast.success('Theme saved successfully!');
    } catch (error) {
      showToast.error('Failed to save theme');
      console.error('Theme save error:', error);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = async () => {
    // Clear preview from sessionStorage
    sessionStorage.removeItem(PREVIEW_THEME_KEY);
    sessionStorage.removeItem(PREVIEW_DESCRIPTION_KEY);
    setGeneratedTheme(null);
    setDescription('');
    // Reload theme from database or reset to default
    try {
      const response = await themeApi.getCurrent();
      applyTheme(response.themeData);
    } catch {
      resetTheme();
    }
  };

  const handleRestore = async () => {
    if (!confirm('Are you sure you want to restore the default theme? This will delete your saved theme.')) {
      return;
    }

    setIsResetting(true);
    try {
      await themeApi.delete();
      resetTheme();
      showToast.success('Theme restored to defaults');
    } catch (error) {
      showToast.error('Failed to restore theme');
      console.error('Theme restore error:', error);
    } finally {
      setIsResetting(false);
    }
  };

  return (
    <MainLayout>
      <div className="max-w-4xl mx-auto">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Customize Theme</h1>
          <p className="text-gray-600">
            Describe your ideal theme and let AI create a custom color palette for you
          </p>
        </div>

        <div className="mb-6">
          <InfoBox
            type="tips"
            items={[
              'Be specific about the mood and atmosphere you want',
              'Mention color preferences or inspirations',
              'Consider mentioning light vs dark themes',
              'Reference other apps or designs you like',
            ]}
          />
        </div>

        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Describe Your Theme
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Example: A dark professional theme with purple accents, inspired by midnight coding sessions..."
            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
            rows={5}
            disabled={isGenerating || generatedTheme !== null}
          />
          <div className="flex items-center justify-between mt-4">
            <span className="text-sm text-gray-500">
              {description.length}/1000 characters (minimum 10)
            </span>
            {!generatedTheme && (
              <div className="flex gap-3">
                <Button
                  onClick={handleRestore}
                  variant="danger"
                  disabled={!hasCustomTheme || isResetting || isGenerating || isSaving}
                >
                  {isResetting ? 'Restoring...' : 'Restore to Defaults'}
                </Button>
                <Button
                  onClick={handleGenerate}
                  disabled={isGenerating || description.length < 10}
                  variant="primary"
                >
                  {isGenerating ? 'Generating...' : 'Generate Theme'}
                </Button>
              </div>
            )}
          </div>
        </div>

        {generatedTheme && (
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4">Preview</h2>
            <p className="text-gray-600 mb-6">
              The theme has been applied to the entire app. Navigate around to see how it looks!
            </p>

            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.primaryColor }}
                />
                <p className="text-xs font-medium text-gray-700">Primary</p>
                <p className="text-xs text-gray-500">{generatedTheme.primaryColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.secondaryColor }}
                />
                <p className="text-xs font-medium text-gray-700">Secondary</p>
                <p className="text-xs text-gray-500">{generatedTheme.secondaryColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.accentColor }}
                />
                <p className="text-xs font-medium text-gray-700">Accent</p>
                <p className="text-xs text-gray-500">{generatedTheme.accentColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.backgroundColor }}
                />
                <p className="text-xs font-medium text-gray-700">Background</p>
                <p className="text-xs text-gray-500">{generatedTheme.backgroundColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.sidebarColor }}
                />
                <p className="text-xs font-medium text-gray-700">Sidebar</p>
                <p className="text-xs text-gray-500">{generatedTheme.sidebarColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.cardBackground }}
                />
                <p className="text-xs font-medium text-gray-700">Card</p>
                <p className="text-xs text-gray-500">{generatedTheme.cardBackground}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.textColor }}
                />
                <p className="text-xs font-medium text-gray-700">Text</p>
                <p className="text-xs text-gray-500">{generatedTheme.textColor}</p>
              </div>
              <div className="text-center">
                <div
                  className="w-full h-20 rounded-lg mb-2 border border-gray-200"
                  style={{ backgroundColor: generatedTheme.borderColor }}
                />
                <p className="text-xs font-medium text-gray-700">Border</p>
                <p className="text-xs text-gray-500">{generatedTheme.borderColor}</p>
              </div>
            </div>

            <div className="flex gap-3 justify-end">
              <Button
                onClick={handleCancel}
                variant="secondary"
                disabled={isSaving}
              >
                Cancel
              </Button>
              <Button
                onClick={handleSave}
                variant="primary"
                disabled={isSaving}
              >
                {isSaving ? 'Saving...' : 'Save Theme'}
              </Button>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
}
