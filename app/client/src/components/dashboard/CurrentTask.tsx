interface CurrentTaskProps {
  task: string;
}

export function CurrentTask({ task }: CurrentTaskProps) {
  return (
    <div className="bg-green-50 border border-green-200 rounded-lg p-6 mb-8">
      <div className="flex items-start gap-3">
        <div className="animate-spin h-5 w-5 border-2 border-green-600 border-t-transparent rounded-full mt-0.5" />
        <div className="flex-1">
          <h2 className="text-lg font-semibold text-green-900 mb-1">Current Task</h2>
          <p className="text-green-700">{task}</p>
        </div>
      </div>
    </div>
  );
}
