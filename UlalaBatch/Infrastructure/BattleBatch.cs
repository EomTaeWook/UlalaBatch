﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UlalaBatch.Models;

namespace UlalaBatch.Infrastructure
{
    public class BattleBatch
    {
        private List<CharacterInfoModel> _sortCharacterInfoModels;
        private HashSet<string> _includeBattleNickname = new HashSet<string>();
        private volatile int _isProcess = 0;
        public void Init(IEnumerable<CharacterInfoModel> characterInfoModels)
        {
            this._sortCharacterInfoModels = characterInfoModels.OrderByDescending(r => r.CombatPower).ToList();
            _includeBattleNickname.Clear();
        }
        public ObservableCollection<BatchResultModel> Batch()
        {
            var result = new ObservableCollection<BatchResultModel>();
            if (this._sortCharacterInfoModels == null)
            {
                return result;
            }

            if (Interlocked.Increment(ref _isProcess) == 1)
            {
                var index = 0;
                var increase = 1;
                var position = Position.Max;
                for (; ; )
                {
                    if (_includeBattleNickname.Count == this._sortCharacterInfoModels.Count)
                    {
                        break;
                    }
                    if(position == Position.Defence && index == 0)
                    {
                        break;
                    }
                    if (index == 0)
                    {
                        position = Position.Elite;
                    }
                    else if (index <= Consts.MaxPositionIndex && position == Position.Elite)
                    {
                        position = Position.Attack;
                    }
                    else if (index > Consts.MaxPositionIndex && position == Position.Attack)
                    {
                        position = Position.Defence;
                        index--;
                        increase = -1;
                    }

                    var batchModel = new BatchResultModel
                    {
                        Tanker = FindTopTanker(position),
                        Healer = FindTopHealer(position),
                        Dealer1 = FindTopDealer(null, position)
                    };

                    if (batchModel.Dealer1 != null)
                    {
                        batchModel.Dealer2 = FindTopDealer(batchModel.Dealer1, position);
                    }
                    if(batchModel.Dealer2 == null)
                    {
                        batchModel.Dealer2 = FindTopDealer(null, position);
                    }
                    
                    batchModel.Position = position;
                    batchModel.Index = index;
                    int sumCombatPower = 0;
                    if (batchModel.Tanker != null)
                    {
                        sumCombatPower += batchModel.Tanker.CombatPower;
                    }
                    if (batchModel.Dealer1 != null)
                    {
                        sumCombatPower += batchModel.Dealer1.CombatPower;
                    }
                    if (batchModel.Dealer2 != null)
                    {
                        sumCombatPower += batchModel.Dealer2.CombatPower;
                    }
                    if (batchModel.Healer != null)
                    {
                        sumCombatPower += batchModel.Healer.CombatPower;
                    }
                    batchModel.CombatPower = sumCombatPower;
                    index += increase;

                    result.Add(batchModel);
                }
                Interlocked.Decrement(ref _isProcess);

                return result;
            }
            return result;
        }
        private CharacterInfoModel FindTopTanker(Position position)
        {
            for (int i=0; i< this._sortCharacterInfoModels.Count; ++i)
            {
                if(this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Tanker)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    else if (position == Position.Elite && this._sortCharacterInfoModels[i].IsEliteExclusion)
                    {
                        continue;
                    }
                    else if ((position == Position.Attack || position == Position.Elite) && this._sortCharacterInfoModels[i].IsOnlyDefence)
                    {
                        continue;
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
        private CharacterInfoModel FindTopDealer(CharacterInfoModel dealer, Position position)
        {
            for (int i = 0; i < this._sortCharacterInfoModels.Count; ++i)
            {
                if (this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Dealer)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    else if (position == Position.Elite && this._sortCharacterInfoModels[i].IsEliteExclusion)
                    {
                        continue;
                    }
                    else if ((position == Position.Attack || position == Position.Elite) && this._sortCharacterInfoModels[i].IsOnlyDefence)
                    {
                        continue;
                    }
                    if (dealer != null)
                    {
                        if(dealer.JobType == this._sortCharacterInfoModels[i].JobType)
                        {
                            continue;
                        }
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
        private CharacterInfoModel FindTopHealer(Position position)
        {
            for (int i = 0; i < this._sortCharacterInfoModels.Count; ++i)
            {
                if (this._sortCharacterInfoModels[i].JobGroupType == JobGroupType.Healer)
                {
                    if (_includeBattleNickname.Contains(this._sortCharacterInfoModels[i].Nickname))
                    {
                        continue;
                    }
                    else if(position == Position.Elite && this._sortCharacterInfoModels[i].IsEliteExclusion)
                    {
                        continue;
                    }
                    else if ((position == Position.Attack || position == Position.Elite) && this._sortCharacterInfoModels[i].IsOnlyDefence)
                    {
                        continue;
                    }
                    _includeBattleNickname.Add(this._sortCharacterInfoModels[i].Nickname);
                    return this._sortCharacterInfoModels[i];
                }
            }
            return null;
        }
    }
}
