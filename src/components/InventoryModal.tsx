import React, { useState } from 'react';
import { Item } from '../types';
import { Sparkles, Package, Info, Check, X } from 'lucide-react';
import { sound } from '../utils/audio';

interface InventoryModalProps {
  inventory: Item[];
  onUseItem: (item: Item) => void;
  onClose: () => void;
}

export const InventoryModal: React.FC<InventoryModalProps> = ({
  inventory,
  onUseItem,
  onClose
}) => {
  const [selectedItem, setSelectedItem] = useState<Item | null>(inventory[0] || null);
  const [categoryFilter, setCategoryFilter] = useState<'all' | 'relic' | 'material' | 'consumable' | 'key_item' | 'gift'>('all');

  const filteredItems = inventory.filter(item => categoryFilter === 'all' || item.category === categoryFilter);

  const rarityColors = {
    common: 'border-slate-700 text-slate-300',
    rare: 'border-sky-500/60 text-sky-300 bg-sky-950/20',
    celestial: 'border-amber-500/60 text-amber-300 bg-amber-950/20',
    ancient: 'border-purple-500/60 text-purple-300 bg-purple-950/20'
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 md:p-6 bg-slate-950/85 backdrop-blur-md">
      <div className="relative w-full max-w-4xl h-[85vh] bg-slate-900/95 border border-slate-700/80 rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/70">
          <div className="flex items-center gap-3">
            <Package className="text-amber-400" size={22} />
            <div>
              <h2 className="font-cinzel text-lg font-bold text-amber-200">Aetherial Satchel & Relics</h2>
              <p className="text-xs text-slate-400">Inventory, artifacts, botanical herbs, and gifts</p>
            </div>
          </div>
          <button onClick={onClose} className="p-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300">
            <X size={18} />
          </button>
        </div>

        {/* Category Filter Tabs */}
        <div className="flex items-center gap-2 px-6 py-3 border-b border-slate-800/80 bg-slate-950/40 text-xs overflow-x-auto">
          {(['all', 'key_item', 'relic', 'material', 'consumable', 'gift'] as const).map(cat => (
            <button
              key={cat}
              onClick={() => {
                setCategoryFilter(cat);
                sound.playClick();
              }}
              className={`px-3 py-1.5 rounded-xl capitalize font-medium transition ${
                categoryFilter === cat
                  ? 'bg-amber-500/20 text-amber-300 border border-amber-500/40 font-semibold'
                  : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800'
              }`}
            >
              {cat.replace('_', ' ')}
            </button>
          ))}
        </div>

        {/* Content Layout: Item Grid on Left, Inspector on Right */}
        <div className="flex-1 flex flex-col md:flex-row overflow-hidden">
          {/* Items Grid */}
          <div className="flex-1 p-6 overflow-y-auto grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
            {filteredItems.map(item => {
              const isSelected = selectedItem?.id === item.id;
              return (
                <button
                  key={item.id}
                  onClick={() => {
                    setSelectedItem(item);
                    sound.playClick();
                  }}
                  className={`relative p-3.5 rounded-2xl border text-left flex flex-col justify-between transition-all ${
                    isSelected
                      ? 'border-amber-400 bg-slate-800/80 shadow-lg scale-102'
                      : 'border-slate-800 bg-slate-950/50 hover:border-slate-700'
                  }`}
                >
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-3xl">{item.icon}</span>
                    <span className="text-xs font-mono font-bold text-amber-400 bg-slate-900 px-2 py-0.5 rounded-full border border-slate-800">
                      x{item.quantity}
                    </span>
                  </div>
                  <div>
                    <div className="font-cinzel text-xs font-bold text-slate-100 truncate">{item.name}</div>
                    <span className={`text-[10px] uppercase font-semibold px-1.5 py-0.5 rounded border mt-1 inline-block ${rarityColors[item.rarity]}`}>
                      {item.rarity}
                    </span>
                  </div>
                </button>
              );
            })}
          </div>

          {/* Right Inspector Panel */}
          {selectedItem && (
            <div className="w-full md:w-80 bg-slate-950/60 border-t md:border-t-0 md:border-l border-slate-800 p-6 flex flex-col justify-between overflow-y-auto">
              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <span className="text-4xl p-2 rounded-2xl bg-slate-900 border border-slate-800">{selectedItem.icon}</span>
                  <div>
                    <h3 className="font-cinzel text-base font-bold text-amber-200">{selectedItem.name}</h3>
                    <span className={`text-[10px] uppercase font-semibold px-2 py-0.5 rounded border inline-block ${rarityColors[selectedItem.rarity]}`}>
                      {selectedItem.rarity} {selectedItem.category}
                    </span>
                  </div>
                </div>

                <div>
                  <h4 className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-1">Description</h4>
                  <p className="text-xs text-slate-200 leading-relaxed">{selectedItem.description}</p>
                </div>

                <div className="bg-slate-900/80 p-3 rounded-xl border border-slate-800/80">
                  <h4 className="text-[11px] font-semibold text-amber-300 uppercase tracking-wider mb-1 flex items-center gap-1.5">
                    <Info size={12} /> Lore Inscription
                  </h4>
                  <p className="text-xs text-slate-400 italic leading-relaxed">{selectedItem.lore}</p>
                </div>

                {selectedItem.effectDescription && (
                  <div className="bg-emerald-950/20 border border-emerald-500/30 p-2.5 rounded-xl text-xs text-emerald-300">
                    <span className="font-semibold">Effect:</span> {selectedItem.effectDescription}
                  </div>
                )}
              </div>

              {selectedItem.usable && (
                <div className="pt-4">
                  <button
                    onClick={() => {
                      onUseItem(selectedItem);
                      sound.playQuestFanfare();
                    }}
                    className="w-full py-2.5 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold text-xs transition shadow-lg"
                  >
                    Activate / Consume Item
                  </button>
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
