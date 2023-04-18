namespace States {
	public interface IState {
		/// <summary>
		/// The property should always return nameof(successor type).
		/// This is required for registration the state and get it more easily
		/// </summary>
		public string Name { get; }
		public bool CanEnter { get; }
		public bool CanExit { get; }
		
		public void Enter();
		public void Exit();
	}
}