import { create } from 'zustand';

interface UIState {
  isSidebarOpen: boolean;
  isTransitioning: boolean;
  isModalOpen: boolean;
  modalContent: React.ReactNode | null;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  openModal: (content: React.ReactNode) => void;
  closeModal: () => void;
  reset: () => void;
}

const getInitialSidebarState = () => {
  if (typeof window === 'undefined') return true;
  return window.innerWidth >= 768;
};

export const useUIStore = create<UIState>((set) => ({
  isSidebarOpen: getInitialSidebarState(),
  isTransitioning: false,
  isModalOpen: false,
  modalContent: null,
  toggleSidebar: () => {
    set({ isTransitioning: true });
    set((state) => ({ isSidebarOpen: !state.isSidebarOpen }));
    setTimeout(() => set({ isTransitioning: false }), 300);
  },
  setSidebarOpen: (open) => set({ isSidebarOpen: open }),
  openModal: (content) => set({ isModalOpen: true, modalContent: content }),
  closeModal: () => set({ isModalOpen: false, modalContent: null }),
  reset: () =>
    set({
      isSidebarOpen: getInitialSidebarState(),
      isTransitioning: false,
      isModalOpen: false,
      modalContent: null,
    }),
}));
