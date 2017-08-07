namespace PLib.FiniteStateMachine
{
    /// <summary>
    /// 2016-16-16
    /// Defines a finite state machine.
    /// </summary>
    public class FiniteStateMachine
    {
        public IState state
        {
            get;
            protected set;
        }

        public virtual void Init(IState state)
        {
            this.state = state;
            this.state.Enter();
        }

        public virtual void Update()
        {
            this.state.Update();
            if (this.state.IsRequestStateChange())
            {
                ChangeTo(this.state.GetNextState());
            }
        }

        public virtual void ChangeTo(IState state)
        {
            this.state.Exit();
            this.state = state;
            this.state.Enter();
        }
    }

    #region Interfaces

    /// <summary>
    /// Interface for the states of a statemachine
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        bool IsRequestStateChange();
        IState GetNextState();
        void Exit();
    }

    #endregion
}