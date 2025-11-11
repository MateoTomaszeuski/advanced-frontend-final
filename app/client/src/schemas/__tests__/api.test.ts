import { describe, it, expect } from 'vitest';
import { 
  UserSchema, 
  ConversationSchema, 
  AgentActionSchema,
  CreateSmartPlaylistRequestSchema,
  PlaylistPreferencesSchema,
} from '../../schemas/api';

describe('API Schemas', () => {
  describe('UserSchema', () => {
    it('should validate correct user data', () => {
      const validUser = {
        id: 1,
        email: 'test@example.com',
        displayName: 'Test User',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      const result = UserSchema.safeParse(validUser);
      expect(result.success).toBe(true);
    });

    it('should reject invalid email', () => {
      const invalidUser = {
        id: 1,
        email: 'not-an-email',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      const result = UserSchema.safeParse(invalidUser);
      expect(result.success).toBe(false);
    });

    it('should allow optional displayName', () => {
      const userWithoutName = {
        id: 1,
        email: 'test@example.com',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      const result = UserSchema.safeParse(userWithoutName);
      expect(result.success).toBe(true);
    });
  });

  describe('ConversationSchema', () => {
    it('should validate correct conversation data', () => {
      const validConversation = {
        id: 1,
        userId: 1,
        title: 'Test Conversation',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      const result = ConversationSchema.safeParse(validConversation);
      expect(result.success).toBe(true);
    });

    it('should allow optional actionCount', () => {
      const conversationWithCount = {
        id: 1,
        userId: 1,
        title: 'Test',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
        actionCount: 5,
      };

      const result = ConversationSchema.safeParse(conversationWithCount);
      expect(result.success).toBe(true);
    });
  });

  describe('AgentActionSchema', () => {
    it('should validate correct action data', () => {
      const validAction = {
        id: 1,
        conversationId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed',
        createdAt: '2024-01-01T00:00:00Z',
      };

      const result = AgentActionSchema.safeParse(validAction);
      expect(result.success).toBe(true);
    });

    it('should validate all status types', () => {
      const statuses = ['Processing', 'Completed', 'Failed', 'AwaitingApproval'];

      statuses.forEach(status => {
        const action = {
          id: 1,
          conversationId: 1,
          actionType: 'Test',
          status,
          createdAt: '2024-01-01T00:00:00Z',
        };

        const result = AgentActionSchema.safeParse(action);
        expect(result.success).toBe(true);
      });
    });

    it('should reject invalid status', () => {
      const invalidAction = {
        id: 1,
        conversationId: 1,
        actionType: 'Test',
        status: 'InvalidStatus',
        createdAt: '2024-01-01T00:00:00Z',
      };

      const result = AgentActionSchema.safeParse(invalidAction);
      expect(result.success).toBe(false);
    });
  });

  describe('CreateSmartPlaylistRequestSchema', () => {
    it('should validate correct request', () => {
      const validRequest = {
        conversationId: 1,
        prompt: 'Create a rock playlist',
      };

      const result = CreateSmartPlaylistRequestSchema.safeParse(validRequest);
      expect(result.success).toBe(true);
    });

    it('should reject empty prompt', () => {
      const invalidRequest = {
        conversationId: 1,
        prompt: '',
      };

      const result = CreateSmartPlaylistRequestSchema.safeParse(invalidRequest);
      expect(result.success).toBe(false);
    });

    it('should allow optional preferences', () => {
      const requestWithPrefs = {
        conversationId: 1,
        prompt: 'Test',
        preferences: {
          maxTracks: 50,
        },
      };

      const result = CreateSmartPlaylistRequestSchema.safeParse(requestWithPrefs);
      expect(result.success).toBe(true);
    });
  });

  describe('PlaylistPreferencesSchema', () => {
    it('should validate correct preferences', () => {
      const validPrefs = {
        maxTracks: 50,
        genres: ['rock', 'pop'],
        mood: 'energetic',
        minEnergy: 50,
        maxEnergy: 100,
        minTempo: 120,
        maxTempo: 180,
      };

      const result = PlaylistPreferencesSchema.safeParse(validPrefs);
      expect(result.success).toBe(true);
    });

    it('should reject maxTracks over 100', () => {
      const invalidPrefs = {
        maxTracks: 150,
      };

      const result = PlaylistPreferencesSchema.safeParse(invalidPrefs);
      expect(result.success).toBe(false);
    });

    it('should reject maxTracks under 1', () => {
      const invalidPrefs = {
        maxTracks: 0,
      };

      const result = PlaylistPreferencesSchema.safeParse(invalidPrefs);
      expect(result.success).toBe(false);
    });

    it('should reject energy values outside 0-100', () => {
      const invalidPrefs = {
        minEnergy: -10,
        maxEnergy: 150,
      };

      const result = PlaylistPreferencesSchema.safeParse(invalidPrefs);
      expect(result.success).toBe(false);
    });

    it('should reject tempo values outside 0-300', () => {
      const invalidPrefs = {
        minTempo: -10,
        maxTempo: 400,
      };

      const result = PlaylistPreferencesSchema.safeParse(invalidPrefs);
      expect(result.success).toBe(false);
    });
  });
});
