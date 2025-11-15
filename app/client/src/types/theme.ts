export interface ThemeData {
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  backgroundColor: string;
  textColor: string;
  sidebarColor: string;
  cardBackground: string;
  borderColor: string;
}

export interface ThemeResponse {
  themeData: ThemeData;
  description: string;
  createdAt: string;
  updatedAt: string;
}

export interface GenerateThemeRequest {
  description: string;
}

export interface SaveThemeRequest {
  themeData: ThemeData;
  description: string;
}
