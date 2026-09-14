import { useState } from "react";
import { LandingPage } from "./components/LandingPage";
import { Dashboard } from "./components/Dashboard";

export default function App() {
  const [activePillar, setActivePillar] = useState<string | null>(null);

  return (
    <div className="app-shell">
      {activePillar === "ingestion" ? (
        <Dashboard onBack={() => setActivePillar(null)} />
      ) : (
        <LandingPage onSelectPillar={setActivePillar} />
      )}
    </div>
  );
}
