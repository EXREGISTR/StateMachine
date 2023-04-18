using System;
using System.Collections;
using System.Collections.Generic;

namespace States {
	public sealed class StateMachine<TState> : IEnumerable<TState> where TState: IState {
		private readonly Dictionary<string, TState> registeredStatesMap;

		public TState Current { get; private set; }

		public StateMachine(int startCapacity = 4) {
			registeredStatesMap = new Dictionary<string, TState>(startCapacity);
		}

		/// <exception cref="ArgumentException">If state already contains in state machine</exception>
		public void Register(TState state) {
			string name = state.Name;
			if (registeredStatesMap.ContainsKey(name)) {
				throw new ArgumentException(
					$"State with name {name} already registered in the state machine {ToString()}!",
					state.ToString());
			}
			
			registeredStatesMap[name] = state;
		}

		public void Unregister(TState state) => registeredStatesMap.Remove(state.Name);

		/// <exception cref="NotFoundException">If newState doesn't contains in the state machine</exception>>
		public T Get<T>() where T: TState {
			var name = typeof(T).Name;
			return (T)Get(name);
		}
		
		/// <param name="safeTransition">Should be check for:
		/// 1) compare name current state and T state;
		/// 2) may exit from current state;
		/// 3) may enter in T state</param>
		/// <returns>True, if succeeded to set state</returns>
		/// <exception cref="NotFoundException">If newState doesn't contains in the state machine</exception>>
		public bool Set<T>(bool safeTransition = true) where T: TState {
			var newState = Get(typeof(T).Name);
			return SetState(newState, safeTransition);
		}

		/// <param name="newState">New state that should be registered in the state machine</param>
		/// <param name="safeTransition">Should be check for:
		/// 1) compare name current state and newState;
		/// 2) may exit from current state;
		/// 3) may enter in newState</param>
		/// <returns>True, if succeeded to set state</returns>
		/// <exception cref="NotFoundException">If newState doesn't contains in the state machine</exception>>
		public bool Set(TState newState, bool safeTransition = true) {
			string stateName = newState.Name;
			if (!registeredStatesMap.ContainsKey(stateName)) {
				throw new NotFoundException(
					$"You are trying to set a state {stateName} that doesn't exist " +
				                            $"in the state machine {ToString()}!");
			}
			
			return SetState(newState, safeTransition);
		}

		public bool IsActiveState(string stateName) => Current.Name == stateName;
		public bool IsActiveState(TState state) => IsActiveState(state.Name);
		public IEnumerable<TState> GetAllStates() => registeredStatesMap.Values;
		
		private bool SetState(TState newState, bool safeTransition) {
			if (safeTransition) {
				if (Current != null) {
					if (Current.Name == newState.Name) return false;
					if (!Current.CanExit) return false;
				}
				
				if (!newState.CanEnter) return false;
			}

			Transition(newState);
			return true;
		}
		
		private TState Get(string name) {
			if (!registeredStatesMap.TryGetValue(name, out TState foundedState)) {
				throw new NotFoundException($"State with name {name} doesn't registered!");
			}
			
			return foundedState;
		}
		
		private void Transition(TState newState) {
			Current?.Exit();
			Current = newState;
			Current.Enter();
		}

		public IEnumerator<TState> GetEnumerator() => GetAllStates().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class NotFoundException : Exception {
		public NotFoundException() { }
		public NotFoundException(string message) : base(message) { }
	}
}
