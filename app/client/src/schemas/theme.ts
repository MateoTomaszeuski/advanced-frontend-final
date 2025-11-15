import { z } from 'zod';

const hexColorRegex = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;

export const themeDataSchema = z.object({
  primaryColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  secondaryColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  accentColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  backgroundColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  textColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  sidebarColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  cardBackground: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
  borderColor: z.string().regex(hexColorRegex, 'Must be a valid hex color'),
});

export const generateThemeRequestSchema = z.object({
  description: z.string().min(10).max(1000),
});

export const saveThemeRequestSchema = z.object({
  themeData: themeDataSchema,
  description: z.string().min(10).max(1000),
});

export const themeResponseSchema = z.object({
  themeData: themeDataSchema,
  description: z.string(),
  createdAt: z.string(),
  updatedAt: z.string(),
});
